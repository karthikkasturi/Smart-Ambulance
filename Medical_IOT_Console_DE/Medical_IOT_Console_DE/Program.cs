using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System.Device.Location;



namespace Medical_IOT_Console_DE
{
    class Program
    {
        static ServiceClient serviceClient;
        static DateTime startingDateTimeUtc;
        static string connectionString = "HostName=SmartAmbulanceIoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=3pe5WVXLOTOdDTEqxR6hVmmZqersKv2B8482bWDeE54=";
        static Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
        static Dictionary<string, Queue<int>> patientsHistory = new Dictionary<string, Queue<int>>();

        static void Main(string[] args)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            ConnectCloud();
            Console.ReadLine();
        }



        public static void ConnectCloud()
        {
            string iotHubD2cEndpoint = "messages/events";
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            startingDateTimeUtc = DateTime.Now;

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach (string partition in d2cPartitions)
            {
                var receiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, startingDateTimeUtc);
                Task.Run(() => ReceiveMessagesFromDeviceAsync(receiver));
            }
        }

        public static async Task ReceiveMessagesFromDeviceAsync(EventHubReceiver receiver)
        {
            List<string> eneryString = new List<string>();
            while (true)
            {
                EventData eventData = await receiver.ReceiveAsync();

                if (eventData == null)
                {
                    continue;
                }
                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                if (data.StartsWith("ambulance"))
                {
                    string[] value = data.Split(',');
                    LoadAmbulanceData(value[0], Convert.ToDouble(value[1]), Convert.ToDouble(value[2]));
                }

                if (data.StartsWith("Medical_IOT"))
                {
                    string[] patientData = data.Split(',');
                    AnalysePatientData(patientData[0], patientData[1], patientData[2], patientData[3], patientData[4]);
                }

                Console.WriteLine(data);
            }
        }

        public static async void AnalysePatientData(string deviceID, string patientName, string pulse, string latitude, string longitude)
        {
            int pulseReading = Convert.ToInt32(pulse);
            string ambulancedeviceID = null;

            if (!patientsHistory.Keys.Contains(patientName))
            {
                var history = new Queue<int>();
                for (int i = 0; i < 3; i++) history.Enqueue(80);
                for (int i = 0; i < 2; i++) history.Enqueue(120);
                patientsHistory.Add(patientName, history);
            }

            var medicalhistory = patientsHistory[patientName];
            medicalhistory.Dequeue();
            medicalhistory.Enqueue(pulseReading);

            if (medicalhistory.All(x => x > 100))
            {
                double distancebtwnpoints = double.MaxValue;
                Console.WriteLine("Patient " + patientName + " in danger.");
                foreach (var entry in dictionary)
                {
                    string key = entry.Key;
                    List<double> values = entry.Value;

                    var startCoord = new GeoCoordinate(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
                    var endCoord = new GeoCoordinate(Convert.ToDouble(values[0]), Convert.ToDouble(values[1]));
                    var distance = startCoord.GetDistanceTo(endCoord);

                    if (distance < distancebtwnpoints)
                    {
                        distancebtwnpoints = distance;
                        ambulancedeviceID = key;
                    }

                    Console.WriteLine(key + " is at distance " + distance);
                }
                if (ambulancedeviceID == null)
                {
                    Console.WriteLine("No ambulances found to send alert to.");
                    return;
                }
                Console.WriteLine("Sent alert to " + ambulancedeviceID + " at a distance of " + distancebtwnpoints);

                string message = "Alert Ambulance to location latitude:" + latitude + " longitude:" + longitude;
                await SendCloudToDeviceMessageAsync(ambulancedeviceID, message);
            }

        }

        public static void LoadAmbulanceData(string key, double latitude, double longitude)
        {
            List<double> list;

            if (dictionary.ContainsKey(key))
            {
                list = dictionary[key];
            }
            else
            {
                list = new List<double>() { 0, 0 };
                dictionary.Add(key, list);
            }
            list[0] = latitude;
            list[1] = longitude;
        }
        
        public async static Task SendCloudToDeviceMessageAsync(string destination, string message)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
            await serviceClient.SendAsync(destination, commandMessage);
        }
        
    }
}
