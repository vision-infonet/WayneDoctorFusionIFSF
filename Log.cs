using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayneDoctorFusionIFSF
{
    public class Log
    {
        #region
        private static string FUSION_ACTIVITY_LOG_PATH = System.Configuration.ConfigurationManager.AppSettings["Location"] + "Logs\\";
        private static long MAX_LOG_FILE_LENGTH = 1000000;
        private delegate void WriteLogDelegate(string s);
        private WriteLogDelegate writeLogDelegate;
        #endregion

        #region methods
        public void ValidateFileSize()
        {
            try
            {
                FileInfo _fileinfo = new FileInfo(FUSION_ACTIVITY_LOG_PATH + "FusionDoctor.log");
                if (_fileinfo.Length > MAX_LOG_FILE_LENGTH)
                    ArchiveLogFile(FUSION_ACTIVITY_LOG_PATH + "FusionDoctor.log");
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
        public void WriteLogInfo(string str)
        {
            try
            {
                ValidateFileSize();
                using (StreamWriter _sw = File.AppendText(System.Configuration.ConfigurationManager.AppSettings["Location"].Replace("Adaptor", string.Empty)
                                       + "\\Logs\\FusionIFSF.log"))
                {
                    _sw.WriteLine(System.DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss:fff ") + str);
                    _sw.Flush();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void WritePollingLogInfo(string str)
        {
            try
            {
                ValidateFileSize();
                using (StreamWriter _sw = File.AppendText(System.Configuration.ConfigurationManager.AppSettings["Location"].Replace("Adaptor", string.Empty)
                                       + "\\Logs\\FusionIFSF_Polling.log"))
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
                using (StreamWriter _sw = File.AppendText(System.Configuration.ConfigurationManager.AppSettings["Location"].Replace("Adaptor", string.Empty)
                                       + "\\Logs\\FusionIFSF_Heartbeat.log"))
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
