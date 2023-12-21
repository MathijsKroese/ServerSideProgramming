using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unsplash;
using Unsplash.Models;

namespace ServerSideProgrammingAssignment
{
    public class GenerateImageSetQueue
    {
        private static HttpClient _httpClient = new();
        private static readonly string _weatherUrl = Environment.GetEnvironmentVariable("WeatherUrl");
        private static readonly string _unsplashKey = Environment.GetEnvironmentVariable("UnsplashKey");
        [FunctionName("GenerateImageSetQueue")]
        public static async Task Run([QueueTrigger("generate-set-queue", Connection = "AzureWebJobsStorage")] string guid,
            [Queue("write-image-queue", Connection = "AzureWebJobsStorage")] CloudQueue queue)
        {
            List<WeatherStation> stations = await GetWeatherStations();
            IEnumerable<Photo.Random> image = await GetRandomImage();

            int count = 0;

            foreach (WeatherStation station in stations)
            {
                string weatherInfo = $"{station.StationName}\n{station.TimeStamp.ToString("dd/MM/yyyy HH:mm")}\n{station.WeatherDescription}";
                string filename = $"{station.StationName}.png";

                if (count <= 2)
                {
                    CloudQueueMessage message = new($"{guid}|{image.FirstOrDefault().Urls.Regular}|{filename}|{weatherInfo}");
                    queue.AddMessage(message);
                }
                count++;
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
        }
    }
}
