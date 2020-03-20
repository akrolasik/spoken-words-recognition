using System;
using System.Threading.Tasks;
using Utils.Interfaces;

namespace Utils.UnitTests.Helpers
{
    public class FileRepositoryValidator : IFileRepository
    {
        private readonly IFileRepository _fileRepository;

        public FileRepositoryValidator(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task UploadFile(string fileContainer, string fromLocalPath, string toSharePath)
        {
            await _fileRepository.UploadFile(fileContainer, fromLocalPath, toSharePath);
        }

        public async Task<bool> DownloadFile(string fileContainer, string fromSharePath, string toLocalPath)
        {
            return await _fileRepository.DownloadFile(fileContainer, fromSharePath, toLocalPath);
        }

        public async Task SetFileContent(string fileContainer, string path, string content)
        {
            await _fileRepository.SetFileContent(fileContainer, path, content);
        }

        public async Task<string> GetFileContent(string fileContainer, string path)
        {
            return await _fileRepository.GetFileContent(fileContainer, path);
        }

        public async Task DeleteFile(string fileContainer, string path)
        {
            await _fileRepository.DeleteFile(fileContainer, path);
        }
    }

    public class FileRepositoryValidator<T> : IFileRepository<T> where T : ITextFile
    {
        private readonly IFileRepository<T> _fileRepository;

        public FileRepositoryValidator(IFileRepository<T> fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task SetFileContent(Guid id, T obj)
        {
            await _fileRepository.SetFileContent(id, obj);
        }

        public async Task<T> GetFileContent(Guid id)
        {
            return await _fileRepository.GetFileContent(id);
        }

        public async Task DeleteFile(Guid id)
        {
            await _fileRepository.DeleteFile(id);
        }
    }
}