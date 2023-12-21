using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Web;

namespace ServerSideProgrammingAssignment
{
    public static class GiveMeTheWeather
    {
        private static readonly string _retrieveUrl = Environment.GetEnvironmentVariable("RetrieveSet");

        [FunctionName("GiveMeTheWeather")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Queue("generate-set-queue", Connection = "AzureWebJobsStorage")] CloudQueue queue)
        {
            Guid guid = Guid.NewGuid();
            CloudQueueMessage message = new(guid.ToString());
            queue.AddMessage(message);

            string url = $"{_retrieveUrl}?guid={guid}";
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
