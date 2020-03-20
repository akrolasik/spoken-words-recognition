using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utils.Interfaces;
using Utils.Model;

namespace PackData.DataPacker
{
    public class DataPacker : IDataPacker
    {
        public IDataRepository<Recording> RecordingsRepository { get; set; }
        public IDataRepository<DatabaseHistoryPoint> DatabaseHistoryRepository { get; set; }
        public IFileRepository FileRepository { get; set; }
        public ILogger Logger { get; set; }

        public string IndexFileName { get; set; }
        public string PackageFileName { get; set; }

        public List<Recording> Recordings { get; set; }
        public string AudioPath { get; set; }
        public string OutputPath { get; set; }

        private float _packageSizeKb;
        private float _filesSizeInKb;
        private TimeSpan _packingTime;
        private TimeSpan _uploadingTime;

        public DataPacker(
            IDataRepository<Recording> recordingsRepository,
            IDataRepository<DatabaseHistoryPoint> databaseHistoryRepository, 
            IFileRepository fileRepository)
        {
            RecordingsRepository = recordingsRepository;
            FileRepository = fileRepository;
            DatabaseHistoryRepository = databaseHistoryRepository;

            IndexFileName = "index.tsv";
            PackageFileName = "package.zip";

            var root = "d:\\local";

            if (!Directory.Exists(root))
                root = Directory.GetCurrentDirectory();

            AudioPath = Path.Combine(root, "audio");
            OutputPath = Path.Combine(root, "output");
        }

        public async Task DownloadRecordings()
        {
            Recordings = await RecordingsRepository.GetRows();
            Logger?.LogInformation($"Recordings downloaded ({Recordings.Count})");
        }

        public async Task DownloadData()
        {
            if (Directory.Exists(AudioPath))
                Directory.Delete(AudioPath, true);

            Directory.CreateDirectory(AudioPath);

            foreach (var recording in Recordings)
            {
                var fileName = $"{recording.Id}.mp3";
                var localPath = Path.Combine(AudioPath, fileName);
                await FileRepository.DownloadFile("audio", fileName, localPath);
            }

            var files = Directory.GetFiles(AudioPath);
            Logger?.LogInformation($"Audio files downloaded ({files.Length})");
        }

        public Task CreateIndex()
        {
            var lines = Recordings
                .OrderBy(x => $"{(x.Word.Length == 1 ? $"0{x.Word}" : x.Word)}_{x.Accent}")
                .Select(x => x.ToText());
            var content = string.Join("\n", lines);
            var indexFilePath = Path.Combine(AudioPath, IndexFileName);
            File.WriteAllText(indexFilePath, content);

            Logger?.LogInformation("Index created");

            return Task.CompletedTask;
        }

        public Task CreatePackage()
        {
            if (Directory.Exists(OutputPath))
                Directory.Delete(OutputPath, true);

            Directory.CreateDirectory(OutputPath);

            var zipFilePath = Path.Combine(OutputPath, PackageFileName);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ZipFile.CreateFromDirectory(AudioPath, zipFilePath);

            stopwatch.Stop();
            _packingTime = stopwatch.Elapsed;

            var files = Directory.GetFiles(AudioPath);
            _filesSizeInKb = files.Sum(x => new FileInfo(x).Length) / 1024.0f;

            _packageSizeKb = new FileInfo(zipFilePath).Length / 1024.0f;

            Logger?.LogInformation($"Package created ({_packageSizeKb:0.00} KB)");

            return Task.CompletedTask;
        }

        public async Task UploadPackage()
        {
            var zipFilePath = Path.Combine(OutputPath, PackageFileName);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await FileRepository.UploadFile("public", zipFilePath, PackageFileName);

            stopwatch.Stop();
            _uploadingTime = stopwatch.Elapsed;

            Logger?.LogInformation("Package uploaded (https://akrolasikstorage.blob.core.windows.net/public/package.zip)");
        }

        public async Task UploadHistory()
        {
            var databaseHistoryPoint = new DatabaseHistoryPoint
            {
                PackingTime = _packingTime,
                UploadTime = _uploadingTime,
                FilesSizeInKb = _filesSizeInKb,
                PackageSizeInKb = _packageSizeKb,
                NumberOfSpeakers = Recordings.Select(x => x.SpeakerId).Distinct().Count(),
                NumberOfRecordings = Recordings.Count
            };

            await DatabaseHistoryRepository.AddRow(databaseHistoryPoint);

            Logger?.LogInformation("History uploaded");
        }

        public Task Cleanup()
        {
            Directory.Delete(AudioPath, true);
            Directory.Delete(OutputPath, true);

            Logger?.LogInformation("Local files removed");

            return Task.CompletedTask;
        }
    }
}