// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace SmartAgriculture
{

    /// <summary>
    /// This sample illustrates the very basics of a device app sending telemetry. For a more comprehensive device app sample, please see
    /// <see href="https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/master/iot-hub/Samples/device/DeviceReconnectionSample"/>.
    /// </summary>
    internal class Program
    {
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private static string s_connectionString = "HostName=SmartAgriMonitorSystemIOTHub.azure-devices.net;DeviceId=SmartAgriMonitorSystem_SimDevice;SharedAccessKey=Xv62ZSwhrzp2r6HszXJQr34Rto3uNAueRYXxsFvhfwY=";
       
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Smart Agriculture - Simulated device.");

            // This sample accepts the device connection string as a parameter, if present
            ValidateConnectionString(args);

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync(cts.Token);

            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        private static void ValidateConnectionString(string[] args)
        {
            if (args.Any())
            {
                try
                {
                    var cs = IotHubConnectionStringBuilder.Create(args[0]);
                    s_connectionString = cs.ToString();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error: Unrecognizable parameter '{args[0]}' as connection string.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    _ = IotHubConnectionStringBuilder.Create(s_connectionString);
                }
                catch (Exception)
                {
                    Console.WriteLine("This sample needs a device connection string to run. Program.cs can be edited to specify it, or it can be included on the command-line as the only parameter.");
                    Environment.Exit(1);
                }
            }
        }

        // Async method to send simulated telemetry
            private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
             // Initial telemetry values
                
                double minTemperature = 20;
                double maxTemperature = 35;
                double minHumidity = 40;
                double maxHumidity = 80;
                double minSoilMoisture = 0;
                double maxSoilMoisture = 100;
                double minLight = 0;
                double maxLight = 100;
                double minNutrients = 0;
                double maxNutrients = 100;
                double minCO2 = 0;
                double maxCO2 = 100;
                double minRainfall = 0;
                double maxRainfall = 100;
                var rand = new Random();
                 

                while (!ct.IsCancellationRequested)
                {
                    double currentTemperature = minTemperature + rand.NextDouble() * (maxTemperature - minTemperature);
                    double currentHumidity = minHumidity + rand.NextDouble() * (maxHumidity - minHumidity);
                    double currentSoilMoisture = minSoilMoisture + rand.NextDouble() * (maxSoilMoisture - minSoilMoisture);
                    double currentLight = minLight + rand.NextDouble() * (maxLight - minLight);
                    double currentNutrients = minNutrients + rand.NextDouble() * (maxNutrients - minNutrients);
                    double currentCO2 = minCO2 + rand.NextDouble() * (maxCO2 - minCO2);
                    double currentRainfall = minRainfall + rand.NextDouble() * (maxRainfall - minRainfall);

                    // Create JSON message
                    string messageBody = JsonSerializer.Serialize(
                        new
                        {
                            temperature = currentTemperature,
                            humidity = currentHumidity,
                            soilMoisture = currentSoilMoisture,
                            light = currentLight,
                            nutrients = currentNutrients,
                            CO2 = currentCO2,
                            rainfall = currentRainfall
                        });
                    
                    using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                    {
                        ContentType = "application/json",
                        ContentEncoding = "utf-8",
                    };
                
                    // Add custom application properties to the message.
                    // IoT hub can filter on these properties without access to the message body.
                    message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
                    message.Properties.Add("soilMoistureAlert", (currentSoilMoisture < 40) ? "true" : "false");    
                    message.Properties.Add("lightAlert", (currentLight < 70) ? "true" : "false");
                    message.Properties.Add("nutrientsAlert", (currentNutrients < 20) ? "true" : "false");
                    message.Properties.Add("CO2Alert", (currentCO2 > 70) ? "true" : "false");
                    message.Properties.Add("rainfallAlert", (currentRainfall > 20) ? "true" : "false");
                   
                    // Send the telemetry message
                    await s_deviceClient.SendEventAsync(message);
                    Console.WriteLine($"{DateTime.Now}> Sending message: {messageBody}");

                    await Task.Delay(3000);
                }
        }
    }
}