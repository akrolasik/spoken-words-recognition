using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utils.Interfaces;
using Utils.Model;

namespace PackData.DataPacker
{
    public interface IDataPacker
    {
        IDataRepository<Recording> RecordingsRepository { get; set; }
        IDataRepository<DatabaseHistoryPoint> DatabaseHistoryRepository { get; set; }
        IFileRepository FileRepository { get; set; }
        ILogger Logger { get; set; }

        string IndexFileName { get; set; }
        string PackageFileName { get; set; }

        List<Recording> Recordings { get; set; }
        string AudioPath { get; set; }
        string OutputPath { get; set; }

        Task DownloadRecordings();
        Task DownloadData();
        Task CreatePackage();
        Task CreateIndex();
        Task UploadPackage();
        Task UploadHistory();
        Task Cleanup();
    }
}