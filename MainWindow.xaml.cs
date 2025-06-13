using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        
        public MainWindow()
        {
            InitializeComponent();
            fusionIFSF = FusionIFSF.GetInstance();
            fusionIFSF.WindowControlDictionary.Add("lblConnectionStatus", lblConnectionStatus);
            fusionIFSF.WindowControlDictionary.Add("txtBoxRequests", txtBoxRequests);
            fusionIFSF.WindowImageDictionary.Add("gifHeartBeat", gifHeartBeat);
            fusionIFSF.WindowControlDictionary.Add("lblLogonSatus", lblLogonSatus);
            fusionIFSF.WindowControlDictionary.Add("txtBoxReceived", txtBoxReceived);
            fusionIFSF.SetControlValue = this.SetControlValue;
            fusionIFSF.EnableButtonsDelegate = this.EnableButtons;
        }


        public void SetControlValue(object obj, object[] myparams)
        {
            try
            {
                if ((Label)obj == lblConnectionStatus)
                    lblConnectionStatus.Dispatcher.Invoke(new Action(()=> lblConnectionStatus.Content= myparams[0]));
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
                    lblLogonSatus.Dispatcher.Invoke(new Action(()=> lblLogonSatus.Content= myparams[0]));
            }
            catch { }
            try
            {
                if ((TextBox)obj == txtBoxReceived)
                    txtBoxReceived.Dispatcher.Invoke(new Action(()=>txtBoxReceived.Text = myparams[0].ToString()));
            }
            catch { }
            //try
            //{
            //    if ((Label)obj == lblLoadStatus)
            //        lblLoadStatus.Dispatcher.Invoke(new SetControlValueDelegate(SetLoadStatusLabelValue), myparams);
            //}
            //catch { }
            //try
            //{
            //    if ((Label)obj == lblLoadAppsKeysStatus)
            //        lblLoadAppsKeysStatus.Dispatcher.Invoke(new SetControlValueDelegate(SetLoadAppsKeysStatusLabelValue), myparams);
            //}
            //catch { }
        }
        
        private void EnableButtons()
        {
            btnGetDeviceConfig.Dispatcher.Invoke(new Action(()=> btnGetDeviceConfig.IsEnabled = true ));
            btnProducts.Dispatcher.Invoke(new Action(() => btnProducts.IsEnabled = true));
            btnGrades.Dispatcher.Invoke(new Action(() => btnGrades.IsEnabled = true));
            btnTanks.Dispatcher.Invoke(new Action(() => btnTanks.IsEnabled = true));
            btnFuelPoints.Dispatcher.Invoke(new Action(() => btnFuelPoints.IsEnabled = true));
            btnAll.Dispatcher.Invoke(new Action(() => btnAll.IsEnabled = true));
            btnSend.Dispatcher.Invoke(new Action(() => btnSend.IsEnabled = true));
            btnClearRequestBox.Dispatcher.Invoke(new Action(() => btnClearRequestBox.IsEnabled = true));
            btnClearReceivedBox.Dispatcher.Invoke(new Action(() => btnClearReceivedBox.IsEnabled = true));
        }

        private void Button_GetSDeviceConfig_Click(object sender, RoutedEventArgs e)
        {
            fusionIFSF.GetConfiguration();
        }
        private void Button_Products_Click(object sender, RoutedEventArgs e)
        {
            fusionIFSF.BuildProductsCommand();
        }
        private void btnGrades_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnClearReceivedBox_Click(object sender, RoutedEventArgs e)
        {
            SetControlValue(txtBoxReceived, new object[] { string.Empty });
        }
        private void btnClearRequestBox_Click(object sender, RoutedEventArgs e)
        {
            SetControlValue(txtBoxRequests, new object[] { string.Empty });
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtBoxRequests.Text))
            {
                MessageBoxResult dialogResult = (MessageBoxResult)System.Windows.Forms.MessageBox.Show("Are you sure to send the Requests \r\nshown in Request Box?", "", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    fusionIFSF.sendingQueue.Enqueue(txtBoxRequests.Text);
                }
            }
        }

        
    }
}
