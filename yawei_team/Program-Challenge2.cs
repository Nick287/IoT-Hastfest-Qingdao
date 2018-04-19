namespace SimulatedDevice
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    
    public class Program
    {
        private const string IotHubUri = "yaweiIoT.azure-devices.net";//"{iot hub hostname}";
        private const string DeviceKey = "IQnilz+dWa2rSBHXjssdTufNG/WX0rwICA2nLTKAzqM=";
        private const string DeviceId = "myFirstDevice";
        private const double MinTemperature = 20;
        private const double MinHumidity = 60;
        private static readonly Random Rand = new Random();
        private static DeviceClient _deviceClient;
        private static int _messageId = 1;

        public static double NextDouble(Random random, double miniDouble, double maxiDouble)
        {
            if (random != null)
            {
                return random.NextDouble() * (maxiDouble - miniDouble) + miniDouble;
            }
            else
            {
                return 0.0d;
            }
        }
        private static string get_uft8(string unicodeString)
        {
            //这边我以big5转换gb2312为例
            Encoding utf8 = Encoding.UTF8;
            Encoding gb2312 = Encoding.Default;


            byte[] gb2312b = gb2312.GetBytes(unicodeString);
            //关键也就是这句了
            byte[] utf8b = Encoding.Convert(gb2312, utf8, gb2312b);

            string strutf8b = utf8.GetString(utf8b);
            return strutf8b;
        }
        private static object makeData(string trainId,string rideId,string correlationId,string trainName)
        {
            Random r = new Random();
            var GPSData = new
            {
                rideId = rideId,
                trainId = trainId,
                correlationId = correlationId,
                lat = NextDouble(r,0,100),//"44.8547",
                longi = NextDouble(r, -100, 100),//"-93.2428",
                alt = NextDouble(r, 100, 260),//"246.509",
                speed = NextDouble(r, 1, 9),//"4.37",
                vertAccuracy = "4",
                horizAccuracy = "10",
                deviceTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")//"2017-12-06T20:23:43.9790000Z"
            };
            var Accelerometer = new
            {
                rideId = rideId,
                trainId = trainId,
                //name = trainName,
                correlationId = correlationId,
                accelX = NextDouble(r,0,10),//"0.701859",
                accelY = NextDouble(r, 0, 10),//"2.19725",
                accelZ = NextDouble(r, 0, 10),//"1.26033",
                deviceTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")//"2018-01-17T19:06:08.0490000Z"
            };
            var photoTriggered = new
            {
                rideId = rideId,//"61397CA0-89ED-4F8C-8997-86F32AEEBD2E",
                trainId = trainId,
                correlationId = correlationId,
                passengerCount = "30",
                eventType = "PhotoTriggered",
                deviceTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")//"2018-01-17T19:06:08.7490000Z"
            };
            var rideStart = new
            {
                rideId = rideId,//"61397CA0-89ED-4F8C-8997-86F32AEEBD2E",
                trainId = trainId,//"05D8569B-69F9-40C8-B862-E197F9F0331E",
                correlationId = correlationId,
                passengerCount = "30",
                eventType = "RideStart",
                deviceTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")//"2018-01-17T19:06:08.7490000Z"
            };
            var rideEnd = new
            {
                rideId = rideId,//"61397CA0-89ED-4F8C-8997-86F32AEEBD2E",
                trainId = trainId,//"05D8569B-69F9-40C8-B862-E197F9F0331E",
                correlationId = correlationId,
                passengerCount = "30",
                eventType = "RideEnd",
                deviceTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")//"2018-01-17T19:06:08.7490000Z"
            };
            object[] Events = { photoTriggered, rideStart, rideEnd };
            var msg = new
            {
                GPSData = GPSData,
                Accelerometer = Accelerometer,
                Events = Events
            };
            return msg;
        }
        static int cnt = 0;
        private static async void SendDeviceToCloudMessagesAsync()
        {
            string trainId1 = "05D8569B-69F9-40C8-B862-E197F9F0331E";
            string trainId2 = "15D8569B-69F9-40C8-B862-E197F9F0331E";
            string correlationId1 = "BB72B77A-687D-4809-92B0-407EA3633B3C";
            string correlationId2 = "CB72B77A-687D-4809-92B0-407EA3633B3C";
            string rideId1 = Guid.NewGuid().ToString();
            string rideId2 = Guid.NewGuid().ToString();
            while (true)
            {
                if(cnt % 2==0)
                {
                    rideId1 = Guid.NewGuid().ToString();
                    rideId2 = Guid.NewGuid().ToString();
                }
                var msg = makeData(trainId1, rideId1, correlationId1,"第一辆飞车");
                var messageString = JsonConvert.SerializeObject(msg);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                //第二辆
                await Task.Delay(1000);
                
                msg = makeData(trainId2, rideId2, correlationId2, "第二辆飞车");
                messageString = JsonConvert.SerializeObject(msg);
                message = new Message(Encoding.UTF8.GetBytes(messageString));
                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                cnt++;
                await Task.Delay(4000*5);
                
            }
            //while (true)
            //{
            //    var currentTemperature = MinTemperature + Rand.NextDouble() * 15;
            //    var currentHumidity = MinHumidity + Rand.NextDouble() * 20;

            //    var telemetryDataPoint = new
            //    {
            //        messageId = _messageId++,
            //        deviceId = DeviceId,
            //        temperature = currentTemperature,
            //        humidity = currentHumidity,
            //        message = "设备数据："+Guid.NewGuid().ToString()
            //    };
            //    var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            //    var message = new Message(Encoding.ASCII.GetBytes(messageString));
            //    message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

            //    await _deviceClient.SendEventAsync(message);
            //    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

            //    await Task.Delay(1000);
            //}
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Mqtt);
            _deviceClient.ProductInfo = "HappyPath_Simulated-CSharp";

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }
    }
}
