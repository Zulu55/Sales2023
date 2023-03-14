using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Sales.API.Helpers
{
    namespace Sales.API.Helpers
    {
        public class FileStorage : IFileStorage
        {
            private readonly string connectionString;
            public FileStorage(IConfiguration configuration)
            {
                connectionString = configuration.GetConnectionString("AzureStorage")!;
            }

            public async Task RemoveFileAsync(string path, string containerName)
            {
                var client = new BlobContainerClient(connectionString, containerName);
                await client.CreateIfNotExistsAsync();
                var fileName = Path.GetFileName(path);
                var blob = client.GetBlobClient(fileName);
                await blob.DeleteIfExistsAsync();
            }

            public async Task<string> SaveFileAsync(byte[] content, string extention, string containerName)
            {
                var client = new BlobContainerClient(connectionString, containerName);
                await client.CreateIfNotExistsAsync();
                client.SetAccessPolicy(PublicAccessType.Blob);
                var fileName = $"{Guid.NewGuid()}{extention}";
                var blob = client.GetBlobClient(fileName);

                using (var ms = new MemoryStream(content))
                {
                    await blob.UploadAsync(ms);
                }

                return blob.Uri.ToString();
            }
        }
    }
}
