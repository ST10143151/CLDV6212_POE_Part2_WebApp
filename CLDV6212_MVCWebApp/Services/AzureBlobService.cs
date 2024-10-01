using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ABC_Retailers.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "products"; // Ensure this is the correct container name

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure the container exists
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

            // Upload the file to the blob storage and allow overwriting
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteBlobAsync(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1];
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }
}
