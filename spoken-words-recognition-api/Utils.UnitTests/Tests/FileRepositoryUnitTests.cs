using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Utils.Configuration;
using Utils.Interfaces;
using Utils.Repositories;
using Utils.UnitTests.Helpers;

namespace Utils.UnitTests.Tests
{
    [TestFixture(typeof(AzureBlobStorage))]
    [TestFixture(typeof(LocalFileRepository))]
    public class FileRepositoryUnitTests<T>
    {
        private readonly Random _random = new Random();
        private const string TestContainerName = "test-container";

        private IFileRepository _fileRepository;
        private FileRepositoryValidator _fileRepositoryValidator;
        private string _exampleContent;
        private string _exampleLocalPath;
        private string _exampleServerPath;
        private string _examplePath;

        [OneTimeSetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();

            var azureStorageConfig = new AzureStorageConfig();
            configuration.Bind("AzureStorage", azureStorageConfig);

            _fileRepository = (IFileRepository)Activator.CreateInstance(typeof(T), azureStorageConfig);
            _fileRepositoryValidator = new FileRepositoryValidator(_fileRepository);

            _exampleContent = _random.Next().ToString();
            _exampleLocalPath = _random.Next().ToString();
            _exampleServerPath = _random.Next().ToString();
            _examplePath = _random.Next().ToString();

            File.WriteAllText(_exampleLocalPath, _exampleContent);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {

        }

        [Test, Order(1)]
        public async Task UploadFile()
        {
            // Execution

            await _fileRepository.UploadFile(TestContainerName, _exampleLocalPath, _exampleServerPath);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(TestContainerName, _exampleServerPath);
            Assert.AreEqual(_exampleContent, file);
        }

        [Test, Order(2)]
        public async Task UpdateFile()
        {
            // Setup

            _exampleContent = _random.Next().ToString();
            File.WriteAllText(_exampleLocalPath, _exampleContent);

            // Execution

            await _fileRepository.UploadFile(TestContainerName, _exampleLocalPath, _exampleServerPath);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(TestContainerName, _exampleServerPath);
            Assert.AreEqual(_exampleContent, file);
        }

        [Test, Order(3)]
        public async Task DownloadFile()
        {
            // Execution

            var file = await _fileRepository.GetFileContent(TestContainerName, _exampleServerPath);

            // Assertion

            Assert.AreEqual(_exampleContent, file);
        }

        [Test, Order(4)]
        public async Task DownloadNonExistingFile()
        {
            // Setup

            await _fileRepositoryValidator.DeleteFile(TestContainerName, _exampleServerPath);

            // Execution

            var file = await _fileRepository.GetFileContent(TestContainerName, _exampleServerPath);

            // Assertion

            Assert.IsNull(file);
        }

        [Test, Order(5)]
        public async Task SetFileContent()
        {
            // Execution

            await _fileRepository.SetFileContent(TestContainerName, _examplePath, _exampleContent);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(TestContainerName, _examplePath);
            Assert.AreEqual(_exampleContent, file);
        }

        [Test, Order(6)]
        public async Task UpdateFileContent()
        {
            // Setup

            _exampleContent = _random.Next().ToString();

            // Execution

            await _fileRepository.SetFileContent(TestContainerName, _examplePath, _exampleContent);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(TestContainerName, _examplePath);
            Assert.AreEqual(_exampleContent, file);
        }

        [Test, Order(7)]
        public async Task GetFileContent()
        {
            // Execution

            var file = await _fileRepository.GetFileContent(TestContainerName, _examplePath);

            // Assertion

            Assert.AreEqual(_exampleContent, file);
        }

        [Test, Order(8)]
        public async Task DeleteFile()
        {
            // Execution

            await _fileRepository.DeleteFile(TestContainerName, _examplePath);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(TestContainerName, _examplePath);
            Assert.IsNull(file);
        }

        [Test, Order(9)]
        public async Task DeleteNonExistingFile()
        {
            // Execution

            await _fileRepository.DeleteFile(TestContainerName, _examplePath);
        }

        [Test, Order(10)]
        public async Task GetNonExistingFileContent()
        {
            // Execution

            var file = await _fileRepository.GetFileContent(TestContainerName, _examplePath);

            // Assertion

            Assert.IsNull(file);
        }
    }
}