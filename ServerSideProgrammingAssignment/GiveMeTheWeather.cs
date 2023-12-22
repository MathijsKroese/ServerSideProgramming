using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Threading.Tasks;

namespace ServerSideProgrammingAssignment
{
    public static class GiveMeTheWeather
    {
        private static readonly string _retrieveUrl = Environment.GetEnvironmentVariable("RetrieveSet");
        private static readonly string _validCredentials = Environment.GetEnvironmentVariable("ValidCredentials");

        [FunctionName("GiveMeTheWeather")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Queue("generate-set-queue", Connection = "AzureWebJobsStorage")] CloudQueue queue)
        {
            string credentials = req.Headers["credentials"];

            if (string.IsNullOrEmpty(credentials) || !credentials.Equals(_validCredentials))
                return new ContentResult
                {
                    Content = "Please provide valid credentials",
                    ContentType = "text/html",
                    StatusCode = 304
                };

            Guid collection = Guid.NewGuid();
            CloudQueueMessage message = new(collection.ToString());
            queue.AddMessage(message);

            string url = $"{_retrieveUrl}&collection={collection}";
            string response = $"Task started. Click the following link to retrieve the images: <a href=\"{url}\" target=\"_blank\">{url}</a>";

            return new ContentResult
            {
                Content = response,
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }
}
