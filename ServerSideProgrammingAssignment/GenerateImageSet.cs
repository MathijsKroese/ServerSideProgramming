using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unsplash;
using Unsplash.Models;

namespace ServerSideProgrammingAssignment
{
    public static class GenerateImageSet
    {
        private static HttpClient _httpClient = new();
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly string _containerName = Environment.GetEnvironmentVariable("ContainerName");
        private static readonly string _weatherUrl = Environment.GetEnvironmentVariable("WeatherUrl");
        private static readonly string _unsplashKey = Environment.GetEnvironmentVariable("UnsplashKey");

        [FunctionName("GenerateImageSet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            string guid = req.Query["guid"];

            if (string.IsNullOrEmpty(guid))
                return new BadRequestObjectResult("No valid identifier");

            List<WeatherStation> stations = await GetWeatherStations();
            IEnumerable<Photo.Random> image = await GetRandomImage();
            byte[] imageStream = await GetImageStream(image.FirstOrDefault().Urls.Regular);


            BlobContainerClient containerClient = new BlobContainerClient(_connectionString, $"{_containerName}/{guid}");

            int count = 0;

            foreach (WeatherStation station in stations)
            {
                string weatherInfo = $"{station.StationName}\n{station.TimeStamp.ToString("dd/MM/yyyy HH:mm")}\n{station.WeatherDescription}";
                MemoryStream memoryStream = GetEditedImageStream(imageStream, weatherInfo);
                string filename = $"{station.StationName}.png";

                if (count <= 2)
                {
                    BlobClient blobClient = containerClient.GetBlobClient($"{guid}/{filename}");
                    blobClient.Upload(memoryStream);
                }
                count++;
            }

            return new OkObjectResult("");
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

        static async Task<IEnumerable<Photo.Random>> GetRandomImage()
        {
            UnsplashClient unsplash = new(new ClientOptions
            {
                AccessKey = _unsplashKey
            });

            return await unsplash.Photos.GetRandomPhotosAsync();
        }

        static async Task<Byte[]> GetImageStream(string url)
        {
            return await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
        }

        static MemoryStream GetEditedImageStream(byte[] imageBytes, string message)
        {
            Image image = Image.Load(imageBytes);
            Font font = SystemFonts.CreateFont("Arial", 48);
            MemoryStream memoryStream = new();

            image.Clone(img =>
            {
                img.DrawText(new RichTextOptions(font), message, Brushes.Solid(Color.Black), Pens.Solid(Color.Azure));
            }).SaveAsPng(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
