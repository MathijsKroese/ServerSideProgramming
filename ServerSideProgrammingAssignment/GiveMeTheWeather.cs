using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ServerSideProgrammingAssignment
{
    public static class GiveMeTheWeather
    {
        [FunctionName("GiveMeTheWeather")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Queue("generate-set-queue", Connection = "AzureWebJobsStorage")] CloudQueue queue)
        {
            Guid guid = Guid.NewGuid();
            CloudQueueMessage message = new(guid.ToString());
            queue.AddMessage(message);

            return new OkObjectResult(guid);
        }
    }
}
