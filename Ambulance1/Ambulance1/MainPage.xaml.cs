using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ambulance1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Task.Run(() => InitializeLocator());
            RecieveAlert();
        }

        public async Task RecieveAlert() {

            string Alert =await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            status.Text = Alert;
            await RecieveAlert();

        }
        private async void InitializeLocator()
        {
            BasicGeoposition location = new BasicGeoposition();
            location.Latitude = 17.438575;
            location.Longitude = 78.510195;
            SendGeoPosition(location);
            
        }
        
        private async void SendGeoPosition(BasicGeoposition location)
        {
            // The location to reverse geocode.
            while (true)
            {
                string str = "ambulance1" + "," + location.Latitude.ToString() + "," + location.Longitude.ToString();
                await AzureIoTHub.SendDeviceToCloudMessageAsync(str);


                await Task.Delay(10000);
            }
        }
    }
}
