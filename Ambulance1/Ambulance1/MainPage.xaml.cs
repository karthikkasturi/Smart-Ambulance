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

         /*   var userPermission = await Geolocator.RequestAccessAsync();
            switch (userPermission)
            {
                case GeolocationAccessStatus.Allowed:

                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    try
                    {

                        //Status.Text = pos.Coordinate.Latitude.ToString() + pos.Coordinate.Longitude.ToString();
                        location.Latitude = pos.Coordinate.Latitude;
                        location.Longitude = pos.Coordinate.Longitude;
                        location.Altitude = Convert.ToDouble(pos.Coordinate.Altitude);
                        ReverseGeocode(location);
                        MapControl1.LandmarksVisible = true;

                    }
                    catch
                    {
                        status.Text = "I cannot c";
                    }
                    break;

                case GeolocationAccessStatus.Denied:
                    status.Text = "I cannot check the weather if you don't give me the access to your location...";
                    break;

                case GeolocationAccessStatus.Unspecified:
                    status.Text = "I got an error while getting location permission. Please try again...";
                    break;
            }*/
        }
        /* private async void ReverseGeocode(BasicGeoposition location)
        {
            // The location to reverse geocode.
            while (true)
            {
                Geopoint pointToReverseGeocode = new Geopoint(location);

                // Reverse geocode the specified geographic location.
                MapLocationFinderResult result =
                      await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

                // If the query returns results, display the name of the town
                // contained in the address of the first result.
                if (result.Status == MapLocationFinderStatus.Success)
                {
                    status.Text = "town = " +
                    result.Locations[0].Address.Town + "Region = " +
                    result.Locations[0].Address.Region + "Street = " +
                    result.Locations[0].Address.Street + "StreetNumber= " +
                    result.Locations[0].Address.StreetNumber + "Postcode = " +
                    result.Locations[0].Address.PostCode;
                    string str = "Ambulance1"+","+location.Latitude.ToString() +","+ location.Longitude.ToString() ;
                    AzureIoTHub.SendDeviceToCloudMessageAsync(str);
                }
                else
                {
                    status.Text = "Dint found";


                }
                await Task.Delay(10000);
            }
        }*/

        private async void SendGeoPosition(BasicGeoposition location)
        {
            // The location to reverse geocode.
            while (true)
            {
                string str = "Ambulance-1" + "," + location.Latitude.ToString() + "," + location.Longitude.ToString();
                AzureIoTHub.SendDeviceToCloudMessageAsync(str);


                await Task.Delay(10000);
            }
        }
    }
}
