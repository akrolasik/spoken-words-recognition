using System.IO;
using API.UnitTests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Configuration;
using Utils.Interfaces;
using Utils.Model;
using Utils.Repositories;

namespace API.UnitTests.Tests
{
    public class TestStartup : BaseStartup
    {
        public TestStartup()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddApplicationPart(typeof(BaseStartup).Assembly);

            BaseConfigureServices(services);

            services.AddSingleton<IRepositoryConfig, RepositoryConfig>();

            services.AddSingleton<IDataRepository<DatabaseHistoryPoint>, LocalDataRepository<DatabaseHistoryPoint>>();
            services.AddSingleton<IDataRepository<RawRecording>, LocalDataRepository<RawRecording>>();
            services.AddSingleton<IDataRepository<Recording>, LocalDataRepository<Recording>>();
            services.AddSingleton<IDataRepository<ReportedIssue>, LocalDataRepository<ReportedIssue>>();
            services.AddSingleton<IFileRepository<RawFrequencies>, LocalFileRepository<RawFrequencies>>();
            services.AddSingleton<IFileRepository<Frequencies>, LocalFileRepository<Frequencies>>();
            services.AddSingleton<IFileRepository<Image>, LocalFileRepository<Image>>();
            services.AddSingleton<IFileRepository, LocalFileRepository>();

            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            BaseConfigure(app, env);
        }
    }
}