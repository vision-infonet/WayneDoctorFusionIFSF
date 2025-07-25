using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using WpfAnimatedGif;

namespace WayneDoctorFusionIFSF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FusionIFSF fusionIFSF = null;
        private delegate void SetControlValueDelegate(string val);
        private static bool SEND_CONFIG_START_END = false;
              
        public MainWindow()
        {
            InitializeComponent();
            fusionIFSF = FusionIFSF.GetInstance();
            fusionIFSF.WindowControlDictionary.Add("lblConnectionStatus", lblConnectionStatus);
            fusionIFSF.WindowControlDictionary.Add("txtBoxRequests", txtBoxRequests);
            fusionIFSF.WindowImageDictionary.Add("gifHeartBeat", gifHeartBeat);
            fusionIFSF.WindowControlDictionary.Add("lblLogonSatus", lblLogonSatus);
            fusionIFSF.WindowControlDictionary.Add("txtBoxReceived", txtBoxReceived);
            fusionIFSF.WindowControlDictionary.Add("progressBar", progressBar);
            fusionIFSF.SetControlValue = this.SetControlValue;
            //fusionIFSF.EnableButtonsDelegate = this.EnableButtons;
        }


        public void SetControlValue(object obj, object[] myparams)
        {
            try
            {
                if ((Label)obj == lblConnectionStatus)
                    lblConnectionStatus.Dispatcher.Invoke(new Action(() => lblConnectionStatus.Content = myparams[0]));
            }
            catch { }

            try
            {
                if ((Image)obj == gifHeartBeat)
                    gifHeartBeat.Dispatcher.Invoke(new Action(() => gifHeartBeat.Visibility = (myparams[0].ToString() == "Visable") ? Visibility.Visible : Visibility.Hidden));
            }
            catch { }
            try
            {
                if ((TextBox)obj == txtBoxRequests)
                    txtBoxRequests.Dispatcher.Invoke(new Action(() => txtBoxRequests.Text=myparams[0].ToString()));
            }
            catch { }
            try
            {
                if ((Label)obj == lblLogonSatus)
                {
                    lblLogonSatus.Dispatcher.Invoke(new Action(() => lblLogonSatus.Content = myparams[0]));
                    if (myparams[0].ToString().Contains("Success"))
                    {
                        lblLogonSatus.Dispatcher.Invoke(new Action(() => lblLogonSatus.Foreground = Brushes.Green));
                        lblHeartBeat.Dispatcher.Invoke(new Action(() => lblHeartBeat.Foreground = Brushes.Green));
                        lblConnectionStatus.Dispatcher.Invoke(new Action(() => lblConnectionStatus.Foreground = Brushes.Green));
                    }
                    else
                    {
                        lblLogonSatus.Dispatcher.Invoke(new Action(() => lblLogonSatus.Foreground = Brushes.Red));
                        lblHeartBeat.Dispatcher.Invoke(new Action(() => lblHeartBeat.Foreground = Brushes.Red));
                        lblConnectionStatus.Dispatcher.Invoke(new Action(() => lblConnectionStatus.Foreground = Brushes.Red));
                    }
                }
            }
            catch { }
            try
            {
                if ((TextBox)obj == txtBoxReceived)
                {
                    if (string.IsNullOrEmpty(myparams[0].ToString()))
                        txtBoxReceived.Dispatcher.Invoke(new Action(() => txtBoxReceived.Text = string.Empty));
                    else
                    {
                        txtBoxReceived.Dispatcher.Invoke(new Action(() => txtBoxReceived.Text += myparams[0].ToString() + "\r\n"));
                        txtBoxReceived.Dispatcher.Invoke(new Action(() => txtBoxReceived.ScrollToEnd()));
                    }
                }
            }
            catch { }
            try
            {
                if ((ProgressBar)obj == progressBar)
                {
                    if(myparams[0].ToString() == "MAX")
                        progressBar.Dispatcher.Invoke(new Action(() => progressBar.Maximum = double.Parse(myparams[1].ToString())));
                    else if (myparams[0].ToString() == "CLEAR")
                        progressBar.Dispatcher.Invoke(new Action(() => progressBar.Value=0));
                    else
                        progressBar.Dispatcher.Invoke(new Action(() => progressBar.Value = double.Parse(myparams[0].ToString())));
                }
            }
            catch { }
        }
        
        //private void EnableButtons()
        //{
        //    btnGetDeviceConfig.Dispatcher.Invoke(new Action(()=> btnGetDeviceConfig.IsEnabled = true ));
        //    btnProducts.Dispatcher.Invoke(new Action(() => btnProducts.IsEnabled = true));
        //    btnGrades.Dispatcher.Invoke(new Action(() => btnGrades.IsEnabled = true));
        //    btnTanks.Dispatcher.Invoke(new Action(() => btnTanks.IsEnabled = true));
        //    btnFuelPoints.Dispatcher.Invoke(new Action(() => btnFuelPoints.IsEnabled = true));
        //    btnAll.Dispatcher.Invoke(new Action(() => btnAll.IsEnabled = true));
        //    btnSend.Dispatcher.Invoke(new Action(() => btnSend.IsEnabled = true));
        //    btnClearRequestBox.Dispatcher.Invoke(new Action(() => btnClearRequestBox.IsEnabled = true));
        //    btnClearReceivedBox.Dispatcher.Invoke(new Action(() => btnClearReceivedBox.IsEnabled = true));
        //}

        private void Button_GetSDeviceConfig_Click(object sender, RoutedEventArgs e)
        {
            SEND_CONFIG_START_END = false;
            fusionIFSF.GetConfiguration();
            SetControlValue(progressBar, new object[] { "CLEAR" });
        }
        //private void Button_Products_Click(object sender, RoutedEventArgs e)
        //{
        //    SEND_CONFIG_START_END = true;
        //    SetControlValue(txtBoxRequests, new object[] { fusionIFSF.BuildProductsCommand() });
        //}
        //private void btnGrades_Click(object sender, RoutedEventArgs e)
        //{
        //    SEND_CONFIG_START_END = true;
        //    SetControlValue(txtBoxRequests, new object[] { fusionIFSF.BuildGradesCommand() });
        //}
        //private void btnFuelPoints_Click(object sender, RoutedEventArgs e)
        //{
        //    SEND_CONFIG_START_END = true;
        //    SetControlValue(txtBoxRequests, new object[] { fusionIFSF.BuildFuelPointsCommand() });
        //}
        private void btnAll_Click(object sender, RoutedEventArgs e)
        {
            SEND_CONFIG_START_END = true;
            SetControlValue(txtBoxRequests, new object[] { fusionIFSF.BuildAll() });
            SetControlValue(progressBar, new object[] { "CLEAR" });
            SetControlValue(progressBar, new object[] { "MAX",5 });
        }
        private void btnClearReceivedBox_Click(object sender, RoutedEventArgs e)
        {
            SetControlValue(txtBoxReceived, new object[] { string.Empty });
            SetControlValue(progressBar, new object[] { "CLEAR" });
        }
        private void btnClearRequestBox_Click(object sender, RoutedEventArgs e)
        {
            SetControlValue(txtBoxRequests, new object[] { string.Empty });
            SetControlValue(progressBar, new object[] { "CLEAR" });
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (fusionIFSF.LogOn)
            {
                if (!string.IsNullOrEmpty(txtBoxRequests.Text))
                {
                    //MessageBoxResult dialogResult = (MessageBoxResult)System.Windows.Forms.MessageBox.Show("Are you sure to send the Requests \r\nshown in Request Box?", "", System.Windows.Forms.MessageBoxButtons.YesNo);
                    //if (dialogResult == MessageBoxResult.Yes)
                    {
                        if (SEND_CONFIG_START_END)
                        {
                            fusionIFSF.BuildConfigStartEnd();
                            fusionIFSF.sendingQueue.Enqueue(fusionIFSF.preSendingDictionary["START"]);
                            System.Threading.Thread.Sleep(1000);
                        }

                        foreach (string s in txtBoxRequests.Text.Split(new string[] { "<?xml" }, StringSplitOptions.None))
                        {
                            XElement elemnt = null;
                            if (string.IsNullOrEmpty(s))
                                continue;
                            try
                            {
                                elemnt = XElement.Parse($"<?xml {s}");
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Forms.MessageBox.Show($"<?xml {s} is not a valid xml! Error:{ex.ToString()}");
                                break;
                            }
                            if (elemnt != null)
                                fusionIFSF.sendingQueue.Enqueue(elemnt.ToString());
                            System.Threading.Thread.Sleep(1000);
                        }
                        System.Threading.Thread.Sleep(1000);
                        if (SEND_CONFIG_START_END)
                        {
                            fusionIFSF.sendingQueue.Enqueue(fusionIFSF.preSendingDictionary["END"]);
                        }
                    }
                }
            }
            else
                System.Windows.Forms.MessageBox.Show("You cannot send request now!\r\nPlease wait for connecting and logon to FusionIFSF device");
        }

       
    }
}
