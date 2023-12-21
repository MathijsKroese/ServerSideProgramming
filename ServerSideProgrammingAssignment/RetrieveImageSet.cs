using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServerSideProgrammingAssignment
{
    public static class RetrieveImageSet
    {
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly string _containerName = Environment.GetEnvironmentVariable("ContainerName");

        [FunctionName("RetrieveImageSet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            string guid = req.Query["guid"];
            if (string.IsNullOrEmpty(guid))
                return new BadRequestObjectResult("No valid identifier provided");

            BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, _containerName);
            var blobItems = blobContainerClient.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, guid);
            IEnumerable<string> blobUrls = blobItems.Select(blobItem => blobContainerClient.GetBlobClient(blobItem.Name).Uri.AbsoluteUri);

            if (!blobUrls.Any())
                return new NotFoundObjectResult("The request is being processed. Please try again later");

            List<string> formattedUrls = new List<string>();
            foreach (string url in blobUrls)
            {
                string formattedUrl = $"<a href=\"{url}\" target=\"_blank\">{url}</a><br>";
                formattedUrls.Add(formattedUrl);
            }

            string htmlContent = string.Join("", formattedUrls);

            return new ContentResult
            {
                Content = htmlContent,
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }
}
