using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerSideProgrammingAssignment
{
    public static class RetrieveImageSet
    {
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly string _containerName = Environment.GetEnvironmentVariable("ContainerName");
        private static readonly string _sasToken = Environment.GetEnvironmentVariable("SasToken");

        [FunctionName("RetrieveImageSet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            string collection = req.Query["collection"];
            if (string.IsNullOrEmpty(collection))
                return new BadRequestObjectResult("No valid identifier provided");

            BlobServiceClient blobClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobClient.GetBlobContainerClient(_containerName);

            BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, _containerName);
            var blobItems = blobContainerClient.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, collection);
            IEnumerable<string> blobUrls = blobItems.Select(blobItem => blobContainerClient.GetBlobClient(blobItem.Name).Uri.AbsoluteUri);

            if (!blobUrls.Any())
                return new NotFoundObjectResult("The request is being processed. Please try again later");

            List<string> formattedUrls = new List<string>();
            foreach (string url in blobUrls)
            {
                string formattedUrl = $"<a href=\"{url}?{_sasToken}\" target=\"_blank\">{url}</a><br>";
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

        /*
 *target url 
 * https://serversideprogrammib948.blob.core.windows.net/serverside-container/0a8a3f50-fe23-4507-b898-311e132c0dc4/Meetstation Arcen.png
 * 
 * sas url
 * https://serversideprogrammib948.blob.core.windows.net/serverside-container?sp=r&st=2023-12-22T01:48:34Z&se=2024-01-13T09:48:34Z&spr=https&sv=2022-11-02&sr=c&sig=kYOPPH4t0Y53kPYAMRfzBuKEGzj%2BR7gFtyZk5MtAtLo%3D
 * 
 * 
 * sas token
 * sp=r&st=2023-12-22T01:48:34Z&se=2024-01-13T09:48:34Z&spr=https&sv=2022-11-02&sr=c&sig=kYOPPH4t0Y53kPYAMRfzBuKEGzj%2BR7gFtyZk5MtAtLo%3D
 * 
 */
    }
}
