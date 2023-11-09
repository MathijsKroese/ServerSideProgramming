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
    public static class CreateImageSet
    {
        private const string _filename = "test.png";
        private const string _weatherUrl = "https://data.buienradar.nl/2.0/feed/json";
        private const string _unsplashAccessKey = "-sCUwfGsK3zxQ_wDSwHu5yPk-luIv7OywjLCcfdWvJg";
        private static HttpClient _httpClient = new();

        [FunctionName("CreateImageSet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Image> images = new();
            var stations = await GetWeatherStations();
            var image = await GetRandomImage();
            var imageStream = await GetImageStream(image.FirstOrDefault().Urls.Regular);

            foreach (var station in stations)
            {
                string weatherInfo = $"{station.StationName}\n{station.TimeStamp.ToString("dd/MM/yyyy HH:mm")}\n{station.WeatherDescription}";
                var memoryStream = EditImage(imageStream, weatherInfo);
                var img = Image.Load(memoryStream);
                images.Add(img);

                img.Save($"../{_filename}");
            }

            return new OkObjectResult("");
        }

        static async Task<List<WeatherStation>> GetWeatherStations()
        {
            List<WeatherStation> weatherStations = new();

            using HttpResponseMessage response = await _httpClient.GetAsync(_weatherUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            foreach (var currentStation in JObject.Parse(json)["actual"]["stationmeasurements"])
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
                AccessKey = _unsplashAccessKey
            });

            return await unsplash.Photos.GetRandomPhotosAsync();
        }

        static async Task<Byte[]> GetImageStream(string url)
        {
            return await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
        }

        static MemoryStream EditImage(byte[] imageBytes, string message)
        {
            var image = Image.Load(imageBytes);
            var font = SystemFonts.CreateFont("Arial", 48);
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
