using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utils.Configuration;
using Utils.Interfaces;

namespace Utils.Repositories
{
    public class LocalFileRepository : IFileRepository
    {
        public Dictionary<string, string> Files = new Dictionary<string, string>();

        public LocalFileRepository(IRepositoryConfig repositoryConfig) { }

        public Task UploadFile(string fileContainer, string fromLocalPath, string toSharePath)
        {
            Files[toSharePath] = File.ReadAllText(fromLocalPath);
            return Task.CompletedTask;
        }

        public Task<bool> DownloadFile(string fileContainer, string fromSharePath, string toLocalPath)
        {
            var exists = Files.TryGetValue(fromSharePath, out var content);

            if (exists)
                File.WriteAllText(toLocalPath, content);

            return Task.FromResult(exists);
        }

        public Task SetFileContent(string fileContainer, string path, string content)
        {
            Files[path] = content;
            return Task.CompletedTask;
        }

        public Task<string> GetFileContent(string fileContainer, string path)
        {
            Files.TryGetValue(path, out var content);
            return Task.FromResult(content);
        }

        public Task DeleteFile(string fileContainer, string path)
        {
            if (Files.ContainsKey(path)) 
                Files.Remove(path);

            return Task.CompletedTask;
        }
    }

    public class LocalFileRepository<T> : IFileRepository<T> where T : ITextFile
    {
        public Dictionary<Guid, string> Files = new Dictionary<Guid, string>();

        public LocalFileRepository(IRepositoryConfig repositoryConfig) { }

        public Task SetFileContent(Guid id, T obj)
        {
            Files[id] = JsonConvert.SerializeObject(obj);
            return Task.CompletedTask;
        }

        public Task<T> GetFileContent(Guid id)
        {
            Files.TryGetValue(id, out var value);

            if (value != null)
            {
                return Task.FromResult(JsonConvert.DeserializeObject<T>(value));
            }

            return Task.FromResult(default(T));
        }

        public Task DeleteFile(Guid id)
        {
            Files.Remove(id);
            return Task.CompletedTask;
        }
    }
}