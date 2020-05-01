using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PackData.DataPacker;
using Task = System.Threading.Tasks.Task;

namespace PackData
{
    public class Function
    {
        private readonly IDataPacker _dataPacker;

        public Function(IDataPacker dataPacker)
        {
            _dataPacker = dataPacker;
        }

        [FunctionName("PackData")]
        public async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger logger)
        {
            _dataPacker.Logger = logger;

            await _dataPacker.DownloadRecordings();
            await _dataPacker.DownloadData();
            await _dataPacker.CreateIndex();
            await _dataPacker.CreatePackage();
            await _dataPacker.UploadPackage();
            await _dataPacker.UploadHistory();
            await _dataPacker.Cleanup();
        }
    }
}
