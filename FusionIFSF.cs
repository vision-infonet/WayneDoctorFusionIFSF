using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WayneDoctorFusionIFSF
{
   
    public class FusionIFSF 
    {
        #region variables
        public static FusionIFSF fusionIFSF = null;
        private readonly static XmlSerializer _xmlserializerServiceRequest = new XmlSerializer(typeof(ServiceRequest));
        private readonly static XmlSerializer _xmlserializerServiceResponse = new XmlSerializer(typeof(ServiceResponse));
        private readonly static XmlSerializer _xmlserializerPOSMessage = new XmlSerializer(typeof(POSMessage));
        private readonly static XmlSerializer _xmlserializerFDCMessage = new XmlSerializer(typeof(FDCMessage));
        private System.Timers.Timer heartbeatTimer;
        private string heartbeatString = string.Empty;
        private POSMessage hearbeat = null;
        private TcpClient tcpClient = null;
        private bool tcpAlive = false;
        NetworkStream stream = null;
        private byte[] _buffer = new byte[16384];
        private static string xmlns_send_xsd = "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", xmlns_send_xsi = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
        private static string xmlns_receive_xsd = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", xmlns_receive_xsi = "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"";
        private static string PPHRASE = "DresserFusion}123";
        private static string HASHID = "0001";
        private static string APPLICATIONSENDER = "1";
        private static string WORKSTATIONID = System.Net.Dns.GetHostName().Substring(0,8);
        private static string INTERFACEVERSION = "01.00";
        private ServiceRequest serviceRequest = null;
        internal Queue<string> sendingQueue = new Queue<string>();
        private int ReqeustID = 0;
        private ReaderWriterLockSlim requestIDReaderWriterLockSlim = new ReaderWriterLockSlim();
        private ManualResetEvent lostHeartbeatManualResetEvent = new ManualResetEvent(false);
        private static string FUSION_ACTIVITY_LOG_PATH = System.Configuration.ConfigurationManager.AppSettings["Location"] + "Logs\\";
        private static long MAX_LOG_FILE_LENGTH = 1000000;
        private delegate void WriteLogDelegate(string s);
        private WriteLogDelegate writeLogDelegate, writeHeartbeatLogDelegate, writePollingLogDelegate;
        public bool OFFLINE = false;
        internal Dictionary<string, Control> WindowControlDictionary;
        internal Dictionary<string, Image> WindowImageDictionary;
        internal delegate void SetControlValueDelegate(object control, object[] myparams);
        internal SetControlValueDelegate SetControlValue;
        internal delegate void EnableButtons();
        internal EnableButtons EnableButtonsDelegate;
        internal List<Product> products = new List<Product>();
        internal List<Grade> grades = new List<Grade>();
        internal Dictionary<string, string> MeasureUnitDictionary;
        public Dictionary<string, Dictionary<string, Product>> PumpProductDictionary = new Dictionary<string, Dictionary<string, Product>>();
        public enum RequestType
        {
            LogOn,
            LogOff,
            ConfigStart,
            ConfigEnd,
            DefProducts,
            GetProductTable,
            GetFPState,
            AuthoriseFuelPoint,
            ClearFuelSaleTrx,
            ChangeFuelPrice,
            GetTotal,
            GetConfiguration,
            CloseDevice,
            OpenDevice,
            StopForecourt,
            StartForecourt,
            SuspendFuelling,
            ResumeFuelling,
        }

        public enum FDC_State
        {
            FDC_OFFLINE,
            FDC_OUTOFORDER,
            FDC_DISABLED,
            FDC_ERRORSTATE,
            FDC_READY,
            FDC_FUELLING,
            FDC_STARTED,
            FDC_CALLING,
            FDC_CLOSED,
            FDC_SUSPENDED_FUELLING,
            FDC_SUSPENDED_STARTED,
            FDC_AUTHORISED,
        }
        public enum MessageType
        {
            POS_Ready,
            FDC_Ready
        }
        public enum ProductBlend
        {
            non,
            High,
            Low
        }
        #endregion

        #region constructor
        public FusionIFSF()
        {
            try
            {
                System.Configuration.Configuration _config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                _config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (!System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("INTERFACEVERSION"))
                {
                    _config.AppSettings.Settings.Add("INTERFACEVERSION", "01.00");
                    _config.Save();
                }
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                INTERFACEVERSION = System.Configuration.ConfigurationManager.AppSettings["INTERFACEVERSION"];

                if (!Directory.Exists(FUSION_ACTIVITY_LOG_PATH))
                    Directory.CreateDirectory(FUSION_ACTIVITY_LOG_PATH);
                writeLogDelegate = this.WriteLogInfo;
                writeHeartbeatLogDelegate = this.WriteHeartbeatLog;
                heartbeatTimer = new System.Timers.Timer(15000);
                heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
                heartbeatTimer.Start();
                serviceRequest = new ServiceRequest();
                serviceRequest.ApplicationSender = APPLICATIONSENDER;
                serviceRequest.RequestID = GetRequestID();
                serviceRequest.WorkstationID = WORKSTATIONID;
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.Receive));
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.SendingQueueThread));
                LogAcitvity("--------------------- FusionIFSF WayneDoctor Start ---------------------");
                WindowControlDictionary = new Dictionary<string, Control>();
                WindowImageDictionary = new Dictionary<string, Image>();
                MeasureUnitDictionary = new Dictionary<string, string>() { { "G", "gallons" },{"L","liter" },{ "M", "M3" },{"K", "kg"} };

                LoadDataFromDB();
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in FusionIFSF() {ex.ToString()}");
            }
        }

        public static FusionIFSF GetInstance()
        {
            if (fusionIFSF == null)
                fusionIFSF = new FusionIFSF();
            return fusionIFSF;
        }
        #endregion

        #region InnerClass
        [Serializable]
        public class ServiceRequest
        {
            [XmlAttribute]
            public RequestType RequestType { get; set; }
            [XmlAttribute]
            public string ApplicationSender { get; set; }
            [XmlAttribute]
            public string WorkstationID { get; set; }
            [XmlAttribute]
            public string RequestID { get; set; }
            public POSdata POSdata { get; set; }
        }

        [Serializable]
        [XmlRootAttribute("ServiceResponse")]
        public class ServiceResponse
        {
            [XmlAttribute]
            public RequestType RequestType { get; set; }
            [XmlAttribute]
            public string ApplicationSender { get; set; }
            [XmlAttribute]
            public string WorkstationID { get; set; }
            [XmlAttribute]
            public string RequestID { get; set; }
            [XmlAttribute]
            public string OverallResult { get; set; }
            public FDCdata FDCdata { get;set;}
        }

        [Serializable]
        [XmlRootAttribute("FDCMessage")]
        public class FDCMessage
        {
            public FDCdata FDCdata { get; set; }
        }

        [Serializable]
        [XmlRootAttribute("POSMessage")]
        public class POSMessage
        {
            [XmlAttribute]
            public MessageType MessageType { get; set; }
            [XmlAttribute]
            public string ApplicationSender { get; set; }
            [XmlAttribute]
            public string WorkstationID { get; set; }
            [XmlAttribute]
            public string MessageID { get; set; }
            public POSdata POSdata { get; set; }
        }

        [Serializable]
        public class POSdata
        {
            public string POSTimeStamp { get; set; }
            public string InterfaceVersion { get; set; }
            public DeviceClass DeviceClass { get; set; }
            public Product[] Products { get; set; }
            public string Emergencystop { get; set; }
        }

        [Serializable]
        public class FDCdata
        {
            public DateTime FDCTimeStamp { get; set; }
            public string InterfaceVersion { get; set; }
            public string ErrorCode { get; set; }
            public DeviceClass[] Devices;
            public DeviceClass DeviceClass { get; set; }
        }

        [Serializable]
        public class DeviceClass
        {
            [XmlAttribute]
            public string Type { get; set; }
            [XmlAttribute]
            public string DeviceID { get; set; }
            [XmlAttribute]
            public string NozzleNo { get; set; }

            public string MaxTrxAmount { get; set; }
            public string MaxTrxVolume { get; set; }
            public ReleasedProducts ReleasedProducts { get; set; }
            public string ReleaseToken { get; set; }
            public string LockFuelSaleTrx { get; set; }
            public string ReservingDeviceId { get; set; }
            public string FuellingType { get; set; }
            public string DeviceState { get; set; }
            public string LockingApplicationSender { get; set; }
            public Nozzle Nozzle { get; set; }
            [XmlAttribute]
            public string PumpNo { get; set; }
            public string ProductNo { get; set; }
            public string State { get; set; }
            public string Amount { get; set; }
            public string Volume { get; set; }
            public string UnitPrice { get; set; }
            [XmlAttribute]
            public string TransactionSeqNo { get; set; }
            public DeviceClass Deviceclass{ get; set; }
        }

        public class ReleasedProducts
        {
            public Product[] Products { get; set; }
        }

        [Serializable]
        public class Product
        {
            [XmlAttribute]
            public string ID { get; set; }
            //public FuelMode FuelMode { get; set; }
            //public string PositionID { get; set; }
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string UnitOfMeasure { get; set; }
            public string NozzleNo { get; set; }
            public decimal[] FuelPrice { get; set; }
            public ProductBlend productBlend = ProductBlend.non;
            //[XmlAttribute]
            //public decimal UnitPrice { get; set; }//for discount usage
        }

        [Serializable]
        public class Grade
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string ProductHighId { get; set; }
            public string ProductLowId { get; set; }
            public string ProductHighPerc { get; set; }
            public string ProductLowPerc { get; set; }
        }

        public class FuelMode
        {
            [XmlAttribute]
            public string ModeNo { get; set; }
            public string PriceNew { get; set; }
        }
        public class Nozzle
        {
            [XmlAttribute]
            public string NozzleNo { get; set; }
            public string LogicalNozzle { get; set; }
            public string LogicalState { get; set; }
            public string TankLogicalState { get; set; }
        }

        #endregion

        #region methods
        private void HeartbeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                heartbeatTimer.Stop();
                if (!tcpAlive && tcpClient == null || !tcpClient.Connected)
                {
                    ConnectToFusionIFSF();
                }

                if (hearbeat == null)
                {
                    LogHeartbeat($"------------Heartbeat Timer Started-----------");
                    hearbeat = new POSMessage();
                    hearbeat.ApplicationSender = APPLICATIONSENDER;
                    hearbeat.WorkstationID = WORKSTATIONID;
                    hearbeat.MessageType = MessageType.POS_Ready;
                    hearbeat.POSdata = new POSdata();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.HeartbeatMonitor));
                }
                hearbeat.MessageID = GetRequestID();
                hearbeat.POSdata.POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerPOSMessage.Serialize(stringWriter, hearbeat);
                    heartbeatString = stringWriter.ToString();
                }
                //SendRequest(heartbeatString, true);
                sendingQueue.Enqueue(heartbeatString);
            }
            catch (Exception ex)
            {
                LogHeartbeat($"Error in HeartbeatTimer_Elapsed() : {ex.ToString()}");
            }
            finally
            {
                heartbeatTimer.Start();
            }
        }

        private void HeartbeatMonitor(object obj)
        {
            while (true)
            {
                lostHeartbeatManualResetEvent.Reset();
                if (!lostHeartbeatManualResetEvent.WaitOne(46000))
                {
                    //lost heart beat 3 times; assume hearbeat interval 15 seconds.
                    CloseConnection();
                    
                }
            }
        }

        private void LoadDataFromDB()
        {
            try
            {
                using (Data.Database _db = new Data.Database(System.Configuration.ConfigurationManager.AppSettings["DatabaseConnection"]))
                {
                    DataTable dt = _db.ExecuteDataTable("SELECT p.ProdID,g.FullName, p.UOM, g.ID as ID, g.Ratio, p.ProdBlend" 
                                                      + " FROM CSCPump.dbo.Product p RIGHT OUTER JOIN CSCPump.dbo.Grade g "
                                                      +" on p.Stock_Code = g.Stock_Code", Data.CommandType.Text);
                    foreach (DataRow dr in dt.Rows)
                    {
                        ProductBlend _productBlend = ProductBlend.non;
                        Enum.TryParse(dr["ProdBlend"].ToString(), out _productBlend);
                        if (!string.IsNullOrEmpty(dr["ProdID"].ToString()))
                        {
                            products.Add(new Product()
                            {
                                ID = dr["ProdID"].ToString(),
                                Name = dr["FullName"].ToString(),
                                UnitOfMeasure = (MeasureUnitDictionary.ContainsKey(dr["UOM"].ToString())) ?
                                           MeasureUnitDictionary[dr["UOM"].ToString()] : "gallons",
                                productBlend = _productBlend
                            });
                        }
                    }
                    foreach(DataRow dr in dt.Rows)
                    {
                        grades.Add(new Grade()
                        {
                            ID = dr["ID"].ToString(),
                            Name = dr["FullName"].ToString(),
                            ProductHighId = (string.IsNullOrEmpty(dr["ProdID"].ToString()) && dr["Ratio"].ToString() == "0") ?
                               products.FirstOrDefault(x=>x.productBlend== ProductBlend.High).ID : dr["ProdID"].ToString(),
                            ProductHighPerc = (dr["Ratio"].ToString() == "0") ? "100": dr["Ratio"].ToString(),
                            ProductLowId = (string.IsNullOrEmpty(dr["ProdID"].ToString()) && dr["Ratio"].ToString() == "0") ?
                               products.FirstOrDefault(x => x.productBlend == ProductBlend.Low).ID : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in LoadDataFromDB(): {ex.ToString()}");
            }
        }
        #endregion

        #region tcp handling

        private void ConnectToFusionIFSF()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(System.Configuration.ConfigurationManager.AppSettings["FusionIFSF_IP"].ToString(), 
                    int.Parse(System.Configuration.ConfigurationManager.AppSettings["FusionIFSF_PORT"].ToString()));
                stream = tcpClient.GetStream();
                tcpAlive = true;
                LogAcitvity($"ConnectToFusionIFSF:  IP = {System.Configuration.ConfigurationManager.AppSettings["FusionIFSF_IP"].ToString()}" +
                    $" PORT={System.Configuration.ConfigurationManager.AppSettings["FusionIFSF_PORT"].ToString()}");
                SetControlValue.Invoke(WindowControlDictionary["lblConnectionStatus"], new object[] { $"Connected to IP: " +
                    $"{System.Configuration.ConfigurationManager.AppSettings["FusionIFSF_IP"].ToString()}" +
                    $"   Port: {System.Configuration.ConfigurationManager.AppSettings["FusionIFSF_PORT"].ToString()}" });
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ShowHeartBeat));
                ServiceRequest _request = Clone(serviceRequest);
                _request.RequestType = RequestType.LogOn;
                _request.POSdata = new POSdata()
                {
                    InterfaceVersion = INTERFACEVERSION,
                    POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                // Send logon message
                string myqueue = string.Empty;
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerServiceRequest.Serialize(stringWriter, _request);
                    myqueue = stringWriter.ToString();
                }
                sendingQueue.Enqueue(myqueue);

                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                CloseConnection();
                LogAcitvity($"Error in ConnectToFusionIFSF():  {ex.ToString()}");
            }
        }

        public void LogOff()
        {
            try
            {
                ServiceRequest _request = Clone(serviceRequest);
                _request.RequestType = RequestType.LogOff;
                _request.POSdata = new POSdata()
                {
                    POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                // Send logon message
                string myqueue = string.Empty;
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerServiceRequest.Serialize(stringWriter, _request);
                    myqueue = stringWriter.ToString();
                }
                sendingQueue.Enqueue(myqueue);
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in LogOff():  {ex.ToString()}");
            }
        }

        private void Receive(object obj)
        {
            int _receivedLength = 0;
            byte[] _data = null;
            List<string> _xmlsList;
            StringBuilder sb = null;
            while (true)
            {
                _buffer = new byte[6049];
                _xmlsList = new List<string>();
                if (tcpAlive && null != tcpClient && tcpClient.Connected)
                {
                    try
                    {
                        _receivedLength = stream.Read(_buffer,0, _buffer.Length);
                        if (_receivedLength > 0)
                        {
                            _data = new byte[_receivedLength];
                            Array.Copy(_buffer, 0, _data, 0, _receivedLength);
                            ResolveIncomingXmls(_data, ref _xmlsList, ref sb);
                            
                            if (sb.Length > 0)
                            {
                                try
                                {
                                    XElement _xele = XElement.Parse(sb.ToString());
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                                _xmlsList.Add(sb.ToString());
                            }

                            ThreadPool.QueueUserWorkItem(new WaitCallback(HandleReceivedPackage), _xmlsList);
                        }
                    }
                    catch (SocketException ex)
                    {
                        LogAcitvity($"Error in Receive():\r\n{ex.ToString()}");
                        Thread.Sleep(1000);
                        continue;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void HandleReceivedPackage(object obj)
        {
            try
            {
                List<string> _receivedList = (List<string>)obj;
                foreach (string s in _receivedList)
                {
                    if (s.Contains("FDC_Ready"))
                    {
                        lostHeartbeatManualResetEvent.Set();
                        LogHeartbeat($"Receive:\r\n{FormattedXMLLog(s.Replace(xmlns_receive_xsd, string.Empty).Replace(xmlns_receive_xsi, string.Empty))}");
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.ShowHeartBeat));
                    }
                    else if (s.Contains("LogOn"))
                    {
                        using (TextReader r = new StringReader(s))
                        {
                            ServiceResponse resp = (ServiceResponse)_xmlserializerServiceResponse.Deserialize(r);
                            if (resp.OverallResult.ToUpper().Contains("SUCCESS"))
                            {
                                this.EnableButtonsDelegate.Invoke();
                                SetControlValue(WindowControlDictionary["lblLogonSatus"], new object[] { "Logon Success" });
                            }
                        }

                    }
                    else if (s.Contains("GetConfiguration"))//Build PumpProductDictionary map
                    {
                        string temp = $"{FormattedXMLLog(s.Replace(xmlns_receive_xsd, string.Empty).Replace(xmlns_receive_xsi, string.Empty))}";
                        LogAcitvity($"Receive:\r\n{temp}");
                        SetControlValue(WindowControlDictionary["txtBoxReceived"], new object[] { temp });
                        string _pumpid, _nozzleNo, _productNo;
                        List<decimal> _duelprices;
                        XElement x = XElement.Parse(s);
                        XElement[] devicesElments = x.Elements("FDCdata").Elements("DeviceClass").ToArray();
                        foreach (XElement deviceElement in devicesElments)
                        {
                            _pumpid = deviceElement.Attribute("DeviceID").Value.PadLeft(2, '0');
                            XElement[] productElements = x.Elements("FDCdata").Elements("DeviceClass").Elements("Product").ToArray();
                            foreach (XElement productElement in productElements)
                            {
                                _productNo = productElement.Attribute("ProductNo").Value;
                                _duelprices = new List<decimal>();
                                foreach (XElement fuelPriceElement in productElement.Elements("FuelPrice").ToArray())
                                {
                                    _duelprices.Add(decimal.Parse(fuelPriceElement.Value));
                                }
                                //PumpProductDictionary[_pumpid][_productNo].FuelPrice = _duelprices.ToArray();
                            }
                        }

                        devicesElments = x.Elements("FDCdata").Elements("DeviceClass").Elements("DeviceClass").ToArray();
                        foreach (XElement xelem in devicesElments)
                        {
                            _pumpid = xelem.Attribute("DeviceID").Value.PadLeft(2, '0');
                            //if (PumpProductDictionary.ContainsKey(_pumpid))
                            //{
                            //    foreach (XElement nozzleElement in xelem.Elements("Nozzle"))
                            //    {
                            //        _nozzleNo = nozzleElement.Attribute("NozzleNo").Value;
                            //        _productNo = nozzleElement.Element("ProductID").Attribute("ProductNo").Value;
                            //        PumpProductDictionary[_pumpid][_productNo].NozzleNo = _nozzleNo;
                            //    }
                            //}
                        }
                    }
                    else
                        LogAcitvity($"Receive:\r\n{FormattedXMLLog(s.Replace(xmlns_receive_xsd, string.Empty).Replace(xmlns_receive_xsi, string.Empty))}");
                }
                

            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in HandleReceivedPackage(): {ex.ToString()}");
            }
           
        }

        private void ShowHeartBeat(object obj)
        {
            SetControlValue(WindowImageDictionary["gifHeartBeat"], new object[] { "Visable" });
            Thread.Sleep(1800);
            SetControlValue(WindowImageDictionary["gifHeartBeat"], new object[] { "Hidden" });
        }

        private void SendingQueueThread(object obj)
        {
            string _req = string.Empty;
            while (true)
            {
                try
                {
                    if(sendingQueue.Count >0)
                    {
                        _req = sendingQueue.Dequeue();
                        SendRequest(_req, _req.Contains("POS_Ready"), _req.Contains("GetFPState"));
                    }
                }
                catch (Exception ex)
                {
                    LogAcitvity($"Error in SendingQueueThread(): {ex.ToString()} ");
                    continue;
                }
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// recursive funntion
        /// </summary>
        private void ResolveIncomingXmls(byte[] receivedBytes, ref List<string> xmls, ref StringBuilder sb)
        {
            byte[] _receivedmsgbytes = null;
            if (receivedBytes.Length < 4)
                return;
            string _receivemsg = string.Empty;
            try
            {
                byte[] _firstFourBytes = new byte[] { receivedBytes[0], receivedBytes[1], receivedBytes[2], receivedBytes[3] };
                int xmllength = CalculatLengthFromIncomeBytes(_firstFourBytes, 0);
                if (receivedBytes[5] == 1)//indicate the first 22 should be header
                    sb = new StringBuilder();
                else
                {
                    _receivedmsgbytes = new byte[receivedBytes.Length];
                    Array.Copy(receivedBytes, 0, _receivedmsgbytes, 0, receivedBytes.Length);
                    _receivemsg = System.Text.Encoding.ASCII.GetString(_receivedmsgbytes);
                    sb.Append(_receivemsg);
                    //if (!temp.Contains("</ServiceResponse>"))
                    return;//next paritl of xml
                }
                if (receivedBytes.Length < xmllength + 22)
                {
                    _receivedmsgbytes = new byte[receivedBytes.Length - 22];
                    Array.Copy(receivedBytes, 22, _receivedmsgbytes, 0, receivedBytes.Length - 22);
                    _receivemsg = System.Text.Encoding.ASCII.GetString(_receivedmsgbytes);
                    sb.Append(_receivemsg);
                    return;
                }
                _receivedmsgbytes = new byte[xmllength];
                Array.Copy(receivedBytes, 22, _receivedmsgbytes, 0, xmllength);//the header has 22 bytes
                _receivemsg = System.Text.Encoding.ASCII.GetString(_receivedmsgbytes);
                xmls.Add(_receivemsg);

                if (receivedBytes.Length > xmllength + 22)
                {
                    byte[] nextChuckBytes = new byte[receivedBytes.Length - xmllength - 22];
                    Array.Copy(receivedBytes, xmllength + 22, nextChuckBytes, 0, receivedBytes.Length - xmllength - 22);
                    ResolveIncomingXmls(nextChuckBytes, ref xmls, ref sb);
                }
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in ResolveIncomingXmls():  {ex.ToString()}");
                return;
            }
        }
        private void CloseConnection()
        {
            SetControlValue(WindowImageDictionary["gifHeartBeat"], new object[] { "Hidden" });
            SetControlValue.Invoke(WindowControlDictionary["lblConnectionStatus"], new object[] { $"Not Connected" });
            LogOff();
            stream.Close();
            stream.Dispose();
            tcpClient.Close();
            tcpAlive = false;   
        }

        private void SendRequest(string reqest, bool isHearbeat, bool isPolling)
        {
            try
            {
                if (tcpAlive && tcpClient != null && stream != null)
                {
                    string md5Hash = CalculateMD5Hash(reqest + PPHRASE);
                    string fulldata = HASHID + CalculateMD5Hash(reqest + PPHRASE) + reqest;
                    //Length should be calculated for the command only. Do not include Hash or Hash ID in the length calculation
                    string slen = CalculateStrLen(reqest);
                    // Converting Length, hash ID and hash value to BCD
                    string hexString = slen + HASHID + md5Hash; // 
                    byte[] bcdResultArr = ConvertToBCD(hexString);
                    byte[] datatoSend = StringToByteArray(reqest);
                    byte[] finalbytes = new byte[bcdResultArr.Length + datatoSend.Length];
                    Array.Copy(bcdResultArr, 0, finalbytes, 0, bcdResultArr.Length);
                    Array.Copy(datatoSend, 0, finalbytes, bcdResultArr.Length, datatoSend.Length);
                    stream.Write(finalbytes, 0, finalbytes.Length);
                    if (isHearbeat)
                        LogHeartbeat($"Send:\r\n{reqest.Replace(xmlns_send_xsd, string.Empty).Replace(xmlns_send_xsi,string.Empty)}");
                    else
                        LogAcitvity($"Send:\r\n{reqest.Replace(xmlns_send_xsd, string.Empty).Replace(xmlns_send_xsi, string.Empty)}");
                }
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in SendRequest():  {ex.ToString()}");
            }
        }
        #endregion

        #region local methods

        internal void GetConfiguration()
        {
            try
            {
                string myqueue = string.Empty;
                ServiceRequest _request = Clone(serviceRequest);
                _request = Clone(serviceRequest);
                _request.RequestType = RequestType.GetConfiguration;
                _request.POSdata = new POSdata()
                {
                    InterfaceVersion = INTERFACEVERSION,
                    POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerServiceRequest.Serialize(stringWriter, _request);
                    myqueue = stringWriter.ToString();
                }
                SetControlValue(WindowControlDictionary["txtBoxRequests"], new object[] { myqueue });
                //sendingQueue.Enqueue(myqueue);
               
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in GetConfiguration():  {ex.ToString()}");
            }
        }

        private string GetRequestID()
        {
            try
            {
                if (requestIDReaderWriterLockSlim.TryEnterWriteLock(0))
                {
                    if (this.ReqeustID < 60000)
                        ReqeustID++;
                    else
                        ReqeustID = 0;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (requestIDReaderWriterLockSlim.IsWriteLockHeld)
                    requestIDReaderWriterLockSlim.ExitWriteLock();
            }
            return ReqeustID.ToString();
        }

        static byte[] ConvertToBCD(string hexStr)
        {
            byte[] bcdArr = new byte[22]; // Maximum size of the arrary to hold the values for Length, HashID and HashValue
            int j = 0;
            for (int i = 0; i < hexStr.Length; i += 2)
            {
                string hexSegment = hexStr.Substring(i, 2);
                int decimalValue = Convert.ToInt32(hexSegment, 16);
                bcdArr[j] = (byte)decimalValue;
                j++;
            }
            return bcdArr;
        }

        static byte[] StringToByteArray(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        static string CalculateMD5Hash(string input1)
        {
            // Convert the input string to a byte array and compute the hash.
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input1);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string.
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        static string CalculateStrLen(String input)
        {
            int length = input.Length;

            // Convert the length to a 32-bit integer
            byte[] lengthBytes = BitConverter.GetBytes(length);

            // Convert to network byte order (big-endian)
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }
            return BitConverter.ToString(lengthBytes).Replace("-", "");
        }

        internal string FormattedXMLLog(string xml)
        {
            try
            {
                XElement _xele = XElement.Parse(xml);
                return _xele.ToString();
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in FormattedXMLLog({xml}): {ex.ToString()}");
                return string.Empty;
            }

        }

        /// <summary>
        /// Calculate acutall data bytes length from the 4 bytes header.
        /// </summary>
        /// <param name="incomingbytes">All bytes in receive buffer</param>
        /// <returns>Acutall data byte length</returns>
        internal static int CalculatLengthFromIncomeBytes(byte[] incomingbytes, int offset)
        {
            return ((int)incomingbytes[offset] * 256 * 256 * 256 + +(int)incomingbytes[offset + 1] * 256 * 256 + (int)incomingbytes[offset + 2] * 256 + (int)incomingbytes[offset + 3]);
        }

        public ServiceRequest Clone(ServiceRequest reqest)
        {
            ServiceRequest _request = null;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, reqest);
                ms.Position = 0;
                _request = (ServiceRequest)bf.Deserialize(ms);
                ms.Close();
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in Clone():{ex.ToString()}");
            }
            return _request;
        }

        #endregion

        #region handle button events
        internal string BuildConfigStartEnd()
        {
            StringBuilder _sb = new StringBuilder();
            try
            {
                ServiceRequest _request = Clone(serviceRequest);
                _request.RequestType = RequestType.ConfigStart;
                _request.POSdata = new POSdata()
                {
                    InterfaceVersion = INTERFACEVERSION,
                    POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };
                string myqueue = string.Empty;
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerServiceRequest.Serialize(stringWriter, _request);
                    myqueue = stringWriter.ToString();
                }
                _sb.Append(myqueue);

                _request = Clone(serviceRequest);
                _request.RequestType = RequestType.ConfigEnd;
                _request.POSdata = new POSdata()
                {
                    POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerServiceRequest.Serialize(stringWriter, _request);
                    myqueue = stringWriter.ToString();
                }
                _sb.Append($"\r\nMY_REQUESTS\r\n{myqueue}");
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in BuildConfigStartEnd(): {ex.ToString()}");
            }
            return _sb.ToString();
        }
        internal void BuildProductsCommand()
        {
            try
            {
                string _startEnd = BuildConfigStartEnd();
                ServiceRequest _request = Clone(serviceRequest);
                _request.RequestType = RequestType.DefProducts;
                _request.POSdata = new POSdata()
                {
                    POSTimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Products = products.ToArray()
                };
                string myqueue = string.Empty;
                using (StringWriter stringWriter = new StringWriter())
                {
                    _xmlserializerServiceRequest.Serialize(stringWriter, _request);
                    myqueue = stringWriter.ToString();
                }
                SetControlValue(WindowControlDictionary["txtBoxRequests"], 
                    new object[] { _startEnd.Replace("MY_REQUESTS", myqueue.Replace(" <Products>", string.Empty).Replace(" </Products>", string.Empty)) });
            }
            catch (Exception ex)
            {
                LogAcitvity($"Error in BuildProductsCommand(): {ex.ToString()}");
            }
        }
        #endregion

        #region log management
        public void ValidateFileSize()
        {
            try
            {
                FileInfo _fileinfo = new FileInfo(FUSION_ACTIVITY_LOG_PATH + "WayneDoctor.log");
                if (_fileinfo.Length > MAX_LOG_FILE_LENGTH)
                    ArchiveLogFile(FUSION_ACTIVITY_LOG_PATH + "WayneDoctor.log");
                _fileinfo = new FileInfo(FUSION_ACTIVITY_LOG_PATH + "WayneDoctor_Heartbeat.log");
                if (_fileinfo.Length > MAX_LOG_FILE_LENGTH)
                    ArchiveLogFile(FUSION_ACTIVITY_LOG_PATH + "WayneDoctor_Heartbeat.log");
                _fileinfo = new FileInfo(FUSION_ACTIVITY_LOG_PATH + "WayneDoctor_Polling.log");
                if (_fileinfo.Length > MAX_LOG_FILE_LENGTH)
                    ArchiveLogFile(FUSION_ACTIVITY_LOG_PATH + "WayneDoctor_Polling.log");
            }
            catch (Exception ex)
            {

            }
        }

        public void ArchiveLogFile(string sourcefile)
        {
            try
            {
                string _destinationfile = sourcefile.Insert(sourcefile.IndexOf("."), System.DateTime.Now.ToString("_yyyyMMddHHmmss"));
                System.IO.File.Move(sourcefile, _destinationfile);
                System.IO.File.SetAttributes(_destinationfile, System.IO.FileAttributes.Normal);
            }
            catch (Exception ex)
            {

            }
        }

        public void LogAcitvity(string s)
        {
            writeLogDelegate.BeginInvoke(s, null, null);
        }
        public void LogHeartbeat(string s)
        {
            writeHeartbeatLogDelegate.BeginInvoke(s, null, null);
        }
        public void WriteLogInfo(string str)
        {
            try
            {
                ValidateFileSize();
                using (StreamWriter _sw = File.AppendText(System.Configuration.ConfigurationManager.AppSettings["Location"]
                                       + "\\Logs\\WayneDoctor.log"))
                {
                    _sw.WriteLine(System.DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss:fff ") + str);
                    _sw.Flush();
                }
            }
            catch (Exception ex)
            {

            }
        }

        
        public void WriteHeartbeatLog(string str)
        {
            try
            {
                ValidateFileSize();
                using (StreamWriter _sw = File.AppendText(System.Configuration.ConfigurationManager.AppSettings["Location"]
                                       + "\\Logs\\WayneDoctor_Heartbeat.log"))
                {
                    _sw.WriteLine(System.DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss:fff ") + str);
                    _sw.Flush();
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion
    }
}
