using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using System.Linq;

namespace ServerSideProgrammingAssignment
{
    public static class RetrieveImageSet
    {
        private static HttpClient _httpClient = new();
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly string _containerName = Environment.GetEnvironmentVariable("ContainerName");
        private static readonly string _weatherUrl = Environment.GetEnvironmentVariable("WeatherUrl");

        [FunctionName("RetrieveImageSet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {

            string guid = req.Query["guid"];
            if (string.IsNullOrEmpty(guid))
                return new BadRequestObjectResult("No images found for this identifier");

            List<WeatherStation> stations = await GetWeatherStations();
            BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, _containerName);

            var blobItems = blobContainerClient.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, guid);
            List<string> imageUrls = blobItems.Select(blobItem => blobContainerClient.GetBlobClient(blobItem.Name).Uri.AbsoluteUri).ToList();

            return new OkObjectResult(imageUrls);
        }

        static async Task<List<WeatherStation>> GetWeatherStations()
        {
            List<WeatherStation> weatherStations = new();

            using HttpResponseMessage response = await _httpClient.GetAsync(_weatherUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            JObject token = JObject.Parse(json);

            foreach (JToken currentStation in token["actual"]["stationmeasurements"])
                weatherStations.Add(new WeatherStation
                {
                    StationName = (string)currentStation["stationname"],
                    TimeStamp = (DateTime)currentStation["timestamp"],
                    WeatherDescription = (string)currentStation["weatherdescription"],
                });

            return weatherStations;
        }
    }
}
