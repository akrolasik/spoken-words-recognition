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
    [TestFixture(typeof(AzureBlobStorage<ExampleTextFile>))]
    [TestFixture(typeof(LocalFileRepository<ExampleTextFile>))]
    public class GenericFileRepositoryUnitTests<T>
    {
        private readonly Random _random = new Random();

        private IFileRepository<ExampleTextFile> _fileRepository;
        private FileRepositoryValidator<ExampleTextFile> _fileRepositoryValidator;
        private ExampleTextFile _exampleTextFile;
        private Guid _exampleId;

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

            _fileRepository = (IFileRepository<ExampleTextFile>) Activator.CreateInstance(typeof(T), azureStorageConfig);
            _fileRepositoryValidator = new FileRepositoryValidator<ExampleTextFile>(_fileRepository);

            _exampleTextFile = GetNewExampleFile();
            _exampleId = Guid.NewGuid();
        }

        [Test, Order(1)]
        public async Task SetFileContent()
        {
            // Execution

            await _fileRepository.SetFileContent(_exampleId, _exampleTextFile);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(_exampleId);
            AssertExtension.AreEqualJson(_exampleTextFile, file);
        }

        [Test, Order(2)]
        public async Task UpdateFileContent()
        {
            // Setup

            _exampleTextFile = GetNewExampleFile();

            // Execution

            await _fileRepository.SetFileContent(_exampleId, _exampleTextFile);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(_exampleId);
            AssertExtension.AreEqualJson(_exampleTextFile, file);
        }

        [Test, Order(3)]
        public async Task GetFileContent()
        {
            // Execution

            var file = await _fileRepository.GetFileContent(_exampleId);

            // Assertion

            AssertExtension.AreEqualJson(_exampleTextFile, file);
        }

        [Test, Order(4)]
        public async Task DeleteFile()
        {
            // Execution

            await _fileRepository.DeleteFile(_exampleId);

            // Assertion

            var file = await _fileRepositoryValidator.GetFileContent(_exampleId);
            Assert.IsNull(file);
        }

        [Test, Order(5)]
        public async Task DeleteNonExistingFile()
        {
            // Execution

            await _fileRepository.DeleteFile(_exampleId);
        }

        [Test, Order(6)]
        public async Task GetNonExistingFileContent()
        {
            // Execution

            var file = await _fileRepository.GetFileContent(_exampleId);

            // Assertion

            Assert.IsNull(file);
        }

        private ExampleTextFile GetNewExampleFile()
        {
            return new ExampleTextFile
            {
                String = _random.Next().ToString(),
                Integer = _random.Next(),
                Boolean = _random.NextDouble() > 0.5,
                Float = (float)_random.NextDouble(),
                Guid = Guid.NewGuid(),
                DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(_random.Next()),
                TimeSpan = TimeSpan.FromSeconds(_random.Next()),
                Enum = (ExampleEnum)_random.Next(3)
            };
        }
    }
}