using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ServerSideProgrammingAssignment
{
    public class GenerateImageSetQueue
    {
        [FunctionName("GenerateImageSetQueue")]
        public void Run([QueueTrigger("generate-set-queue", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            HttpClient client = new();
            string url = $"{Environment.GetEnvironmentVariable("GenerateSet")}&guid={myQueueItem}";
            client.PostAsync(url, null);
        }
    }
}
