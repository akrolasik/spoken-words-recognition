using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackData.DataPacker;
using Utils.Configuration;
using Utils.Interfaces;
using Utils.Model;
using Utils.Repositories;

[assembly: FunctionsStartup(typeof(PackData.Startup))]

namespace PackData
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();

            var azureStorageConfig = configuration.GetSection("AzureStorage").Get<AzureStorageConfig>();
            builder.Services.AddSingleton<IRepositoryConfig>(azureStorageConfig);

            builder.Services.AddSingleton<IDataRepository<DatabaseHistoryPoint>, AzureTableStorage<DatabaseHistoryPoint>>();
            builder.Services.AddSingleton<IDataRepository<Recording>, AzureTableStorage<Recording>>();
            builder.Services.AddSingleton<IFileRepository, AzureBlobStorage>();

            builder.Services.AddSingleton<IDataPacker, DataPacker.DataPacker>();
            builder.Services.AddLogging();
        }
    }
}