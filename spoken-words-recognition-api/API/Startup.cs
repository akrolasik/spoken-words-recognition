using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Utils.Configuration;
using Utils.Interfaces;
using Utils.Model;
using Utils.Repositories;

namespace API
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BaseConfigureServices(services);

            var azureStorageConfig = Configuration.GetSection("AzureStorage").Get<AzureStorageConfig>();
            services.AddSingleton<IRepositoryConfig>(azureStorageConfig);

            services.AddSingleton<IDataRepository<DatabaseHistoryPoint>, AzureTableStorage<DatabaseHistoryPoint>>();
            services.AddSingleton<IDataRepository<RawRecording>, AzureTableStorage<RawRecording>>();
            services.AddSingleton<IDataRepository<Recording>, AzureTableStorage<Recording>>();
            services.AddSingleton<IDataRepository<ReportedIssue>, AzureTableStorage<ReportedIssue>>();

            services.AddSingleton<IFileRepository<RawFrequencies>, AzureBlobStorage<RawFrequencies>>();
            services.AddSingleton<IFileRepository<Frequencies>, AzureBlobStorage<Frequencies>>();
            services.AddSingleton<IFileRepository<Image>, AzureBlobStorage<Image>>();
            services.AddSingleton<IFileRepository, AzureBlobStorage>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "akrolasik/api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            BaseConfigure(app, env);

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "akrolasik/api");
            });
        }
    }
}
