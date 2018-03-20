using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ArduinoReadData
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SerialDevice serialPort = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        List<double> pulseValues = new List<double> { 80, 80, 80, 80, 100, 110, 120, 120, 120, 120 };

        public MainPage()
        {
            this.InitializeComponent();
            

            listOfDevices = new ObservableCollection<DeviceInformation>();
            CheckPortisavaialble();
        }
        private async void CheckPortisavaialble()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for (int i = 0; i < dis.Count; i++)
                {
                    if (dis[i].Id.Contains("USB"))
                        listOfDevices.Add(dis[i]);
                }
                DeviceInformation entry = listOfDevices[0];
                comPortInput(entry);
                CheckPortisavaialblestatus.Text = listOfDevices[0].Id.ToString();
            }
            catch (Exception ex)
            {
                CheckPortisavaialblestatus.Text = ex.Message;
            }


        }
        private async void comPortInput(DeviceInformation entry)
        {
            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                if (serialPort == null) return;
                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(2000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                comPortInputstatus.Text = "Serial port configured successfully";
                // Create cancellation token object to close I/O operations when closing the device

                Listen();
            }
            catch (Exception ex)
            {
                comPortInputstatus.Text = ex.Message;
            }
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }
        private async Task ReadAsync()
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 1024;
            string MedValues = null;




            // Set InputStreamOptions to complete the asynchronous read operation when one or   more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask();

            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                MedValues = rcvdText.Text = dataReaderObject.ReadString(bytesRead);
                status.Text = "bytes read successfully!";
                await AzureIoTHub.SendDeviceToCloudMessageAsync(MedValues);
            }
            await Task.Delay(TimeSpan.FromSeconds(6));
        }
        static int i = 0; 
        private async void btnSendPulseData_Click(object sender, RoutedEventArgs e)
        {
            if (i == pulseValues.Count) i = 0;
            string MedValues = "Medical_IOT_PulseOximeter,Karthik,";
            MedValues += pulseValues[i];
            MedValues += ",17.437462,78.448288";
            status.Text = "Read succesfully";
            await AzureIoTHub.SendDeviceToCloudMessageAsync(MedValues);
            rcvdText.Text = MedValues + ";";
            i++;
        }
    }
}
