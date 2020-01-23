using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Test
{
    class Program
    {
        // Prefix for containers created by the sample.
        private const string ContainerPrefix = "sample-";

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Example Start");
            BasicStorageBlockBlobOperationsAsync().Wait();
            Console.WriteLine("Example End");

            return 1;
        }


        private static async Task BasicStorageBlockBlobOperationsAsync()
        {
            const string ImageToUpload = "HelloWorld.png";
            string containerName = ContainerPrefix + Guid.NewGuid();

            // Retrieve storage account information from connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true"); // Common.CreateStorageAccountFromConnectionString();

            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            Console.WriteLine("1. Creating Container");
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            try
            {
                // The call below will fail if the sample is configured to use the storage emulator in the connection string, but 
                // the emulator is not running.
                // Change the retry policy for this call so that if it fails, it fails quickly.
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                await container.CreateIfNotExistsAsync(requestOptions, null);
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are running with the default connection string, please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            // To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate 
            // access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions 
            // to allow public access to blobs in this container. Uncomment the line below to use this approach. Then you can view the image 
            // using: https://[InsertYourStorageAccountNameHere].blob.core.windows.net/democontainer/HelloWorld.png
            // await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Upload a BlockBlob to the newly created container
            Console.WriteLine("2. Uploading BlockBlob");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(ImageToUpload);

            // Set the blob's content type so that the browser knows to treat it as an image.
            blockBlob.Properties.ContentType = "image/png";
            await blockBlob.UploadFromFileAsync(ImageToUpload);

            // List all the blobs in the container.
            /// Note that the ListBlobs method is called synchronously, for the purposes of the sample. However, in a real-world
            /// application using the async/await pattern, best practices recommend using asynchronous methods consistently.
            Console.WriteLine("3. List Blobs in Container");
            var blobs = await container.ListBlobsSegmentedAsync(new BlobContinuationToken());
            
            foreach (IListBlobItem blob in blobs.Results)
            {
                // Blob type will be CloudBlockBlob, CloudPageBlob or CloudBlobDirectory
                // Use blob.GetType() and cast to appropriate type to gain access to properties specific to each type
                Console.WriteLine("- {0} (type: {1}, container: {2})", blob.Uri, blob.GetType(), blob.Container);
            }

            // Download a blob to your file system
            Console.WriteLine("4. Download Blob from {0}", blockBlob.Uri.AbsoluteUri);
            await blockBlob.DownloadToFileAsync(String.Format("./CopyOf{0}", ImageToUpload), FileMode.Create);

            // Create a read-only snapshot of the blob
            Console.WriteLine("5. Create a read-only snapshot of the blob");
            CloudBlockBlob blockBlobSnapshot = await blockBlob.CreateSnapshotAsync(null, null, null, null);
            Console.WriteLine("- Name: {0}", blockBlobSnapshot.Name);
            // Clean up after the demo. This line is not strictly necessary as the container is deleted in the next call.
            // It is included for the purposes of the example. 
            Console.WriteLine("6. Delete block blob and all of its snapshots");
            await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);

            // Note that deleting the container also deletes any blobs in the container, and their snapshots.
            // In the case of the sample, we delete the blob and its snapshots, and then the container,
            // to show how to delete each kind of resource.
            Console.WriteLine("7. Delete Container");
            await container.DeleteIfExistsAsync();
        }
    }
}
