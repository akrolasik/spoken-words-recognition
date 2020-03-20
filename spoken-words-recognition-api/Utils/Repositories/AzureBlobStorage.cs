using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Utils.Configuration;
using Utils.Extensions;
using Utils.Interfaces;

namespace Utils.Repositories
{
    public class AzureBlobStorage : IFileRepository
    {
        private readonly Dictionary<string, CloudBlobContainer> _cloudBlobs = new Dictionary<string, CloudBlobContainer>();
        private readonly CloudBlobClient _blobClient;

        public AzureBlobStorage(IRepositoryConfig repositoryConfig)
        {
            if (!(repositoryConfig is AzureStorageConfig azureStorageConfig))
            {
                throw new ArgumentNullException("Missing azureStorageConfig");
            }

            azureStorageConfig.AssertAllPropertiesNotNull();

            var storageAccount = CloudStorageAccount.Parse(azureStorageConfig.ConnectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

        public async Task UploadFile(string fileContainer, string fromLocalPath, string toSharePath)
        {
            var blobReference = GetContainer(fileContainer).GetBlockBlobReference(toSharePath);
            await blobReference.UploadFromFileAsync(fromLocalPath);
        }

        public async Task<bool> DownloadFile(string fileContainer, string fromSharePath, string toLocalPath)
        {
            var blobReference = GetContainer(fileContainer).GetBlockBlobReference(fromSharePath);

            var exists = await blobReference.ExistsAsync();

            if (exists)
                await blobReference.DownloadToFileAsync(toLocalPath, FileMode.Create);

            return exists;
        }

        public async Task SetFileContent(string fileContainer, string path, string content)
        {
            var blobReference = GetContainer(fileContainer).GetBlockBlobReference(path);
            await blobReference.UploadTextAsync(content);
        }

        public async Task<string> GetFileContent(string fileContainer, string path)
        {
            var blobReference = GetContainer(fileContainer).GetBlockBlobReference(path);

            var exists = await blobReference.ExistsAsync();

            if (exists)
                return await blobReference.DownloadTextAsync();

            return null;
        }

        public async Task DeleteFile(string fileContainer, string path)
        {
            var blobReference = GetContainer(fileContainer).GetBlockBlobReference(path);
            await blobReference.DeleteIfExistsAsync();
        }

        private CloudBlobContainer GetContainer(string fileContainer)
        {
            if (_cloudBlobs.ContainsKey(fileContainer))
            {
                return _cloudBlobs[fileContainer];
            }

            _cloudBlobs[fileContainer] = _blobClient.GetContainerReference(fileContainer);
            _cloudBlobs[fileContainer].CreateIfNotExistsAsync().Wait();

            return _cloudBlobs[fileContainer];
        }
    }

    public class AzureBlobStorage<T> : IFileRepository<T> where T : ITextFile
    {
        private readonly CloudBlobContainer _cloudBlob;

        public AzureBlobStorage(IRepositoryConfig repositoryConfig)
        {
            if (!(repositoryConfig is AzureStorageConfig azureStorageConfig))
            {
                throw new ArgumentNullException("Missing azureStorageConfig");
            }

            azureStorageConfig.AssertAllPropertiesNotNull();

            var storageAccount = CloudStorageAccount.Parse(azureStorageConfig.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var containerName = typeof(T).Name.PascalToKebabCase();
            _cloudBlob = blobClient.GetContainerReference(containerName);
            _cloudBlob.CreateIfNotExistsAsync().Wait();
        }

        public async Task SetFileContent(Guid id, T obj)
        {
            var blobReference = _cloudBlob.GetBlockBlobReference(id.ToString());
            await blobReference.UploadTextAsync(obj.ToText());
        }

        public async Task<T> GetFileContent(Guid id)
        {
            var blobReference = _cloudBlob.GetBlockBlobReference(id.ToString());

            var exists = await blobReference.ExistsAsync();

            if (exists)
            {
                var content = await blobReference.DownloadTextAsync();
                var instance = Activator.CreateInstance<T>();
                return (T)instance.FromText(content);
            }

            return default;
        }

        public async Task DeleteFile(Guid id)
        {
            var blobReference = _cloudBlob.GetBlockBlobReference(id.ToString());
            await blobReference.DeleteIfExistsAsync();
        }
    }
}