using FunctionApp = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System;

namespace smartagristorageaccount
{
    
    public class SensorData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double SoilMoisture { get; set; }
        public double Light { get; set; }
        public double Nutrients { get; set; }
        public double CO2 { get; set; }
        public double? Rainfall { get; set; }
    }

    public class IoTEventHub_SmartAgri
    {
        private static HttpClient client = new HttpClient();
        
        [FunctionName("IoTEventHub_SmartAgri")]
        public static void Run([EventHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
                      [CosmosDB(databaseName: "IoTData",
                                 collectionName: "SensorData",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out SensorData output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            var jsonBody = Encoding.UTF8.GetString(message.Body);
            dynamic data = JsonConvert.DeserializeObject(jsonBody);
            double? temperature = data.temperature;
            double? humidity = data.humidity;
            double? soilMoisture = data.soilMoisture;
            double? light = data.light;
            double? nutrients = data.nutrients;
            double? rainfall = data.rainfall;
            double? co2 = data.co2;

    output = new SensorData();

    if (temperature.HasValue)
    {
        output.Temperature = (double)temperature.Value;
    }
    if (humidity.HasValue)
    {
        output.Humidity = (double)humidity.Value;
    }
    if (soilMoisture.HasValue)
    {
        output.SoilMoisture = (double)soilMoisture.Value;
    }
       if (light.HasValue)
    {
        output.Light = (double)light.Value;
    }
        if (nutrients.HasValue)
    {
        output.Nutrients = (double)nutrients.Value;
    }
        if (co2.HasValue)
    {
        output.CO2 = (double)co2.Value;
    }
       if (rainfall.HasValue)
    {
        output.Rainfall = (double)rainfall.Value;
    }
    
        output.Id = Guid.NewGuid().ToString();
    
        }
        [FunctionName("GetSensorData")]
      public static IActionResult GetSensorData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SensorData/")] HttpRequest req,
        [CosmosDB(databaseName: "IoTData",
                  collectionName: "SensorData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT * FROM c")] IEnumerable<dynamic> SensorData,
                  ILogger log)
      {
        return new OkObjectResult(SensorData);
      }
    }
}
