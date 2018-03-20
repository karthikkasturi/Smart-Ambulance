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
        static string connectionString = "HostName=SmartAmbulanceHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=80gM2w6+gmpzFgWW9wXvl77wPbBTG5gukYFW1WUqWio=";
        static Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
        static Dictionary<string, Queue<int>> patientsHistory = new Dictionary<string, Queue<int>>();

        static void Main(string[] args)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            
            ConnectCloud();
            //ConnectCloudtogetmedicaldata();
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
            Console.ReadLine();
        }

        public static async Task ReceiveMessagesFromDeviceAsync(EventHubReceiver receiver)
        {
            List<string> eneryString = new List<string>();
            while (true)
            {
                EventData eventData = await receiver.ReceiveAsync();
                if (eventData == null)
                    continue;
                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                //   String temp = data.Replace("\r\n", "").Trim();
                // string[] array = temp.Split(',');
                if (data.Contains("Ambulance"))
                {
                    string[] value = data.Split(',');
                    LoadAmbulanceData(value[0],Convert.ToDouble(value[1]), Convert.ToDouble(value[2]));
                }
                if (data.Contains("Medical_IOT"))
                {
                    string[] patientData = data.Split(',');
                    AnalysePatientData(patientData[0], patientData[1], patientData[2], patientData[3], patientData[4]);
                }
                //DangerWindow(value[0], value[1]);
                Console.WriteLine(data);
            }
        }

        public static async void AnalysePatientData(string deviceID,string patientName,string pulse,string latitude,string longitude)
        {
            //int systolicreading = Convert.ToInt32(systolic);
            //int diastolicreading = Convert.ToInt32(diastolic);
            int pulseReading = Convert.ToInt32(pulse);
            string ambulancedeviceID = null;
            if (!patientsHistory.Keys.Contains(patientName))
            {
                var history = new Queue<int>();
                for(int i=0;i<5;i++)history.Enqueue(80);
                patientsHistory.Add(patientName, history);
            }
            var medicalhistory = patientsHistory[patientName];
            medicalhistory.Dequeue();
            medicalhistory.Enqueue(pulseReading);
            if ( medicalhistory.All(x => x > 100) )
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
                Console.WriteLine("Sent alert to " +ambulancedeviceID + " at a distance of "  + distancebtwnpoints);
                string message = "Alert Ambulance to location latitude:"+ latitude+"longitude:"+longitude;
                await SendCloudToDeviceMessageAsync(ambulancedeviceID, message);
            }

        }
        /*public static void LoadAmbulanceData(string[] value)
        {
            string key=null;
            List<double> list = new List<double>();
            for(int i=0;i<value.Length;i++)
            {
                if (i == 0)
                    key = value[i];
                else {
                    list.Add(Convert.ToDouble(value[i]));
                     }
            }

            if (dictionary.ContainsKey(key))
            {
            }
            else
            {
                dictionary.Add(key, list);
            }
        }
        */
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
            //Console.WriteLine(dictionary);
        }



        public async static Task SendCloudToDeviceMessageAsync(string destination, string message)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
            await serviceClient.SendAsync(destination, commandMessage);
        }


        public static async void ConnectCloudtogetmedicaldata()
        {
            string iotHubD2cEndpoint = "messages/events";
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
            startingDateTimeUtc = DateTime.Today;
            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach (string partition in d2cPartitions)
            {
                var receiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, startingDateTimeUtc);
                await ReceiveMessagesFromDeviceAsync1(receiver);
            }
            Console.ReadLine();
        }

        public static async Task ReceiveMessagesFromDeviceAsync1(EventHubReceiver receiver)
        {
            List<string> eneryString = new List<string>();
            while (true)
            {
                EventData eventData = await receiver.ReceiveAsync();
                if (eventData == null)
                    continue;
                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                
                
                if (data.Contains("Medical_IOT"))
                {
                    string[] patientData = data.Split(',');
                    List<double> list;

                    if (dictionary.ContainsKey(patientData[6]))
                    {
                        list = dictionary[patientData[6]];
                    }
                    else
                    {
                        list = new List<double>();
                        dictionary.Add(patientData[6], list);
                    }
                    list.Add(Convert.ToInt32(patientData[2]));
                    list.Add(Convert.ToInt32(patientData[3]));

                }
                //DangerWindow(value[0], value[1]);
                Console.WriteLine(data);
            }
        }


    }
}
