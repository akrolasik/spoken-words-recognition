using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using PackData.DataPacker;
using Utils.Configuration;
using Utils.Model;
using Utils.Repositories;

namespace PackData.UnitTests
{
    public class DataPackerTests
    {
        private readonly Random _random = new Random();

        private LocalDataRepository<Recording> _recordingsRepository;
        private LocalDataRepository<DatabaseHistoryPoint> _databaseHistoryRepository;
        private LocalFileRepository _fileRepository;

        private IDataPacker _dataPacker;

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

            _recordingsRepository = new LocalDataRepository<Recording>(azureStorageConfig);
            _databaseHistoryRepository = new LocalDataRepository<DatabaseHistoryPoint>(azureStorageConfig);
            _fileRepository = new LocalFileRepository(azureStorageConfig);

            _dataPacker = new DataPacker.DataPacker(_recordingsRepository, _databaseHistoryRepository, _fileRepository);

            var recordings = Enumerable.Range(0, 10).Select(x => GetRandomRecording()).ToList();
            _recordingsRepository.AddRows(recordings);

            foreach (var recording in recordings)
            {
                _fileRepository.Files[$"{recording.Id}.weba"] = _random.Next().ToString();
            }
        }

        [Test, Order(1)]
        public async Task DownloadRecordings()
        {
            // Execution

            await _dataPacker.DownloadRecordings();

            // Assertion

            Assert.IsNotEmpty(_dataPacker.Recordings);

            var expectedRecordings = await _recordingsRepository.GetRows();

            Assert.AreEqual(expectedRecordings.Count, _dataPacker.Recordings.Count);

            foreach (var dataPackerRecording in _dataPacker.Recordings)
            {
                var recording = expectedRecordings.FirstOrDefault(x => x.Id == dataPackerRecording.Id);
                Assert.NotNull(recording);
            }
        }

        [Test, Order(2)]
        public async Task DownloadData()
        {
            // Execution

            await _dataPacker.DownloadData();

            // Assertion

            var files = Directory.GetFiles(_dataPacker.AudioPath);

            Assert.IsNotEmpty(files);

            var expectedFiles = _fileRepository.Files;

            Assert.AreEqual(expectedFiles.Count, files.Length);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                Assert.Contains(fileName, expectedFiles.Keys);
            }
        }

        [Test, Order(3)]
        public async Task CreateIndex()
        {
            // Execution

            await _dataPacker.CreateIndex();

            // Assertion

            var indexFilePath = Path.Combine(_dataPacker.AudioPath, _dataPacker.IndexFileName);
            var indexFile = new FileInfo(indexFilePath);

            Assert.True(indexFile.Exists);

            var content = File.ReadAllText(indexFilePath);

            Assert.NotNull(content);
            content = string.Join("\n", content.Split('\n').Skip(1));

            var expectedRecordings = await _recordingsRepository.GetRows();
            var expectedContent = string.Join("\n", expectedRecordings.Select(x => x.ToText()));

            Assert.AreEqual(expectedContent, content);
        }

        [Test, Order(4)]
        public async Task CreatePackage()
        {
            // Execution

            await _dataPacker.CreatePackage();

            // Assertion

            var zipFilePath = Path.Combine(_dataPacker.OutputPath, _dataPacker.PackageFileName);
            var package = new FileInfo(zipFilePath);

            Assert.True(package.Exists);
            Assert.AreEqual(".zip", package.Extension);
        }

        [Test, Order(5)]
        public async Task UploadPackage()
        {
            // Execution

            await _dataPacker.UploadPackage();

            // Assertion

            var zipFilePath = Path.Combine(_dataPacker.OutputPath, _dataPacker.PackageFileName);
            var expectedContent = File.ReadAllText(zipFilePath);

            var packageContent = await _fileRepository.GetFileContent("public", _dataPacker.PackageFileName);
            Assert.NotNull(packageContent);
            Assert.AreEqual(expectedContent, packageContent);
        }

        [Test, Order(6)]
        public async Task UploadHistory()
        {
            // Execution

            await _dataPacker.UploadHistory();

            // Assertion

            var expectedRecordings = await _recordingsRepository.GetRows();
            var historyPoints = await _databaseHistoryRepository.GetRows();

            Assert.IsNotEmpty(historyPoints);

            Assert.AreEqual(expectedRecordings.Count, historyPoints.First().NumberOfSpeakers);
            Assert.AreEqual(expectedRecordings.Count, historyPoints.First().NumberOfRecordings);
        }

        [Test, Order(7)]
        public async Task Cleanup()
        {
            // Execution

            await _dataPacker.Cleanup();

            // Assertion

            var zipFilePath = Path.Combine(_dataPacker.OutputPath, _dataPacker.PackageFileName);
            Assert.False(File.Exists(zipFilePath));
        }

        protected Recording GetRandomRecording()
        {
            return new Recording
            {
                Id = Guid.NewGuid(),
                SpeakerId = Guid.NewGuid(),
                Accent = _random.Next().ToString(),
                Modification = (Modification)_random.Next((int)Modification.Max - 1),
                Word = _random.Next().ToString()
            };
        }
    }
}