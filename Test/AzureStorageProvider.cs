using System;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;


namespace Test
{
    public class AzureStorageProvider : IStorageProvider<Blob, Uri>
    {
        private readonly CloudBlobContainer _cloudBlobContainer;


        public AzureStorageProvider(string connectionString, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();

            Console.WriteLine("1. Creating Container");
            _cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            try
            {
                // The call below will fail if the sample is configured to use the storage emulator in the connection string, but 
                // the emulator is not running.
                // Change the retry policy for this call so that if it fails, it fails quickly.
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                _cloudBlobContainer.CreateIfNotExistsAsync(requestOptions, null);

                var permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                _cloudBlobContainer.SetPermissionsAsync(permissions);
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are running with the default connection string, please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }




            //_cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            
            //if (!_cloudBlobContainer.ExistsAsync().Result)
            //{
            //    CreateContainer();
            //}
        }

        public async Task<Uri> SaveAsync(Blob blob)
        {
            var cloudBlockBlob = _cloudBlobContainer.GetBlockBlobReference(blob.Name);
            cloudBlockBlob.Properties.ContentType = "image/jpg";
            await cloudBlockBlob.UploadFromStreamAsync(blob.Stream);
            
            return cloudBlockBlob.Uri;
        }

        private void CreateContainer()
        {
            _cloudBlobContainer.CreateAsync();
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            _cloudBlobContainer.SetPermissionsAsync(permissions);
        }
    }
}
