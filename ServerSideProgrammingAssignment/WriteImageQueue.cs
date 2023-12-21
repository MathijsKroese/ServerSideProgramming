using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerSideProgrammingAssignment
{
    public class WriteImageQueue
    {
        private static HttpClient _httpClient = new();
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly string _containerName = Environment.GetEnvironmentVariable("ContainerName");

        [FunctionName("WriteImageQueue")]
        public static async Task Run([QueueTrigger("write-image-queue", Connection = "AzureWebJobsStorage")] string message)
        {
            string guid = message.Split('|')[0];
            string imageUrl = message.Split('|')[1];
            string filename = message.Split('|')[2];
            string weatherInfo = message.Split('|')[3];

            var imageStream = await GetImageStream(imageUrl);
            MemoryStream memoryStream = WriteToImage(imageStream, weatherInfo);

            StoreImage(guid, filename, memoryStream);
        }

        private static async Task<Byte[]> GetImageStream(string url)
        {
            return await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
        }

        private static MemoryStream WriteToImage(byte[] imageBytes, string message)
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

        private static void StoreImage(string guid, string filename, MemoryStream memoryStream)
        {
            BlobContainerClient containerClient = new BlobContainerClient(_connectionString, $"{_containerName}");
            BlobClient blobClient = containerClient.GetBlobClient($"{guid}/{filename}");

            blobClient.UploadAsync(memoryStream);
        }
    }
}
