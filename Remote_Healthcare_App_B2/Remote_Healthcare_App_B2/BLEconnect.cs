using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

namespace ErgoConnect
{
    // Original author:
    // Avans TI
    // Changes in structure made by B2. Also added file writing of data.
    class BLEconnect
    {
        public const System.String ergometerSerialLastFiveNumbers = "00472";
        

        public static void Main(string[] args)
        {
            ConnectToErgoAndHR(ergometerSerialLastFiveNumbers);
            Console.Read();
        }

        public static async Task ConnectToErgoAndHR(String ergometerSerialLastFiveNumbers)
        {
            System.Int32 errorCode = 0;

            BLE ergoMeterBle = new BLE();
            BLE heartRateSensorBle = new BLE();

            Thread.Sleep(1000); // Timeout to detect and list Bluetooth devices upon using constructor "new BLE()".

            // To list available devices
            printDevices(ergoMeterBle);

            //---Ergometer Bluetooth Low Energy Code---
            ConnectToErgoMeter(ergoMeterBle, ergometerSerialLastFiveNumbers, errorCode);

            //---Heart rate Bluetooth Low Energy code---
            ConnectToHeartRateSensor(heartRateSensorBle, errorCode);
        }

        private async static void ConnectToErgoMeter(BLE ergoMeterBle, System.String ergometerSerialLastFiveNumbers, System.Int32 errorCode)
        {

            // Attempt to connect to the Ergometer.
            errorCode = await ergoMeterBle.OpenDevice($"Tacx Flux {ergometerSerialLastFiveNumbers}"); // Example: Tacx Flux 01140

            // Receive bluetooth services and print afterwards, error check.
            printServices(ergoMeterBle);

            // Set service
            errorCode = await ergoMeterBle.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");

            // Subscribe 
            ergoMeterBle.SubscriptionValueChanged += Ble_SubscriptionValueChanged;
            errorCode = await ergoMeterBle.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

        }

        private static void writeToFile(System.String path, System.String input)
        {
            System.IO.File.AppendAllText(path, input + "\n");
        }
        private async static void ConnectToHeartRateSensor(BLE heartRateSensorBle, System.Int32 errorCode)
        {

            // Attempt to connect to the heart rate sensor.
            errorCode = await heartRateSensorBle.OpenDevice("Decathlon Dual HR");

            // Set service
            await heartRateSensorBle.SetService("HeartRate");

            // Subscribe
            heartRateSensorBle.SubscriptionValueChanged += Ble_SubscriptionValueChanged;
            await heartRateSensorBle.SubscribeToCharacteristic("HeartRateMeasurement");
        }

        private static void printServices(BLE ergoMeterBle)
        {
            List<BluetoothLEAttributeDisplay> services = ergoMeterBle.GetServices;
            foreach (BluetoothLEAttributeDisplay service in services)
            {
                Console.WriteLine($"Service: {service}");
            }
        }

        private static void printDevices(BLE ergoMeterBle)
        {
            List<String> bluetoothDeviceList = ergoMeterBle.ListDevices();
            Console.WriteLine("Devices currently found:");
            foreach (System.String deviceName in bluetoothDeviceList)
            {
                Console.WriteLine($"Device: {deviceName}");
            }
        }
    
        private static void Ble_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            System.String serviceName = e.ServiceName;
            System.String data = BitConverter.ToString(e.Data).Replace("-", " ");
            System.String UTF8 = Encoding.UTF8.GetString(e.Data);
            Console.WriteLine($"ID:{ergometerSerialLastFiveNumbers} {serviceName} {data}");

            System.String path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/RemoteHealthcare/";
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            writeToFile($"{path}BLEdata.txt",  $"{ergometerSerialLastFiveNumbers} {data}");
        }
    }
}
