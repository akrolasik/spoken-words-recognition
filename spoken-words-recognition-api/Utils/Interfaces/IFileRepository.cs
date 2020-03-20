using System;
using System.Threading.Tasks;

namespace Utils.Interfaces
{
    public interface IFileRepository
    {
        Task UploadFile(string fileContainer, string fromLocalPath, string toSharePath);
        Task<bool> DownloadFile(string fileContainer, string fromSharePath, string toLocalPath);
        Task SetFileContent(string fileContainer, string path, string content);
        Task<string> GetFileContent(string fileContainer, string path);
        Task DeleteFile(string fileContainer, string path);
    }

    public interface IFileRepository<T> where T : ITextFile
    {
        Task SetFileContent(Guid id, T obj);
        Task<T> GetFileContent(Guid id);
        Task DeleteFile(Guid id);
    }
}