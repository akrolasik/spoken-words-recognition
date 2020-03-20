using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using Utils.Interfaces;
using Utils.Model;

namespace API.UnitTests.Tests
{
    [TestFixture]
    public class BaseControllerUnitTests
    {
        protected readonly Random Random = new Random();

        protected IDataRepository<RawRecording> RawRecordingsRepository;
        protected IDataRepository<Recording> RecordingsRepository;
        protected IDataRepository<ReportedIssue> ReportedIssuesRepository;
        protected IFileRepository<RawFrequencies> RawFrequenciesRepository;
        protected IFileRepository<Frequencies> FrequenciesRepository;
        protected IFileRepository<Image> ImagesRepository;
        protected IFileRepository AudioRepository;

        protected IConfiguration Configuration;

        protected TestServer Server;
        protected HttpClient Client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var webHostBuilder = new WebHostBuilder().UseEnvironment("Test").UseStartup<TestStartup>();
            Server = new TestServer(webHostBuilder);
            Client = Server.CreateClient();

            RawRecordingsRepository = (IDataRepository<RawRecording>)Server.Services.GetService(typeof(IDataRepository<RawRecording>));
            RecordingsRepository = (IDataRepository<Recording>)Server.Services.GetService(typeof(IDataRepository<Recording>));
            ReportedIssuesRepository = (IDataRepository<ReportedIssue>)Server.Services.GetService(typeof(IDataRepository<ReportedIssue>));
            RawFrequenciesRepository = (IFileRepository<RawFrequencies>)Server.Services.GetService(typeof(IFileRepository<RawFrequencies>));
            FrequenciesRepository = (IFileRepository<Frequencies>)Server.Services.GetService(typeof(IFileRepository<Frequencies>));
            ImagesRepository = (IFileRepository<Image>)Server.Services.GetService(typeof(IFileRepository<Image>));
            AudioRepository = (IFileRepository)Server.Services.GetService(typeof(IFileRepository));
            Configuration = (IConfiguration)Server.Services.GetService(typeof(IConfiguration));

            var result = Client.GetAsync("health").Result;

            if (result.StatusCode == HttpStatusCode.NotFound)
                Assert.Ignore("API Not Found");
        }

        protected Task<HttpResponseMessage> PutAsync(string requestUri, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return Client.PutAsync(requestUri, content);
        }

        protected RawFrequencies GetRandomRawFrequencies()
        {
            return new RawFrequencies
            {
                FrequenciesChunks = Enumerable.Range(0, 10).Select(x => new FrequenciesChunk
                {
                    Data = Random.Next().ToString()
                }).ToArray()
            };
        }

        protected WordOccurence GetRandomWordOccurence(int milliseconds)
        {
            return new WordOccurence
            {
                Milliseconds = milliseconds,
                Word = new Word
                {
                    Value = Random.Next().ToString(),
                    InWords = Random.Next().ToString(),
                    Pronounce = Random.Next().ToString()
                }
            };
        }

        protected Frequencies GetRandomFrequencies()
        {
            return new Frequencies
            {
                FrequenciesChunks = Enumerable.Range(0, 10).Select(x => new FrequenciesChunk
                {
                    Data = Random.Next().ToString()
                }).ToArray()
            };
        }

        protected WordOccurence GetRandomWordOccurence()
        {
            return new WordOccurence
            {
                Milliseconds = Random.Next(),
                Word = new Word
                {
                    Value = Random.Next().ToString(),
                    InWords = Random.Next().ToString(),
                    Pronounce = Random.Next().ToString()
                }
            };
        }

        protected Recording GetRandomRecording()
        {
            return new Recording
            {
                Id = Guid.NewGuid(),
                SpeakerId = Guid.NewGuid(),
                Accent = Random.Next().ToString(),
                Modification = (Modification)Random.Next((int)Modification.Max - 1),
                Word = Random.Next().ToString()
            };
        }

        protected RawRecording GetRandomRawRecording()
        {
            return new RawRecording
            {
                Id = Guid.NewGuid(),
                SpeakerId = Guid.NewGuid(),
                Accent = Random.Next().ToString(),
                Modification = (Modification)Random.Next((int)Modification.Max - 1),
                Words = Enumerable.Range(0, 10).Select(GetRandomWordOccurence).ToArray()
            };
        }

        protected Image GetRandomImage()
        {
            return new Image
            {
                Width = Random.Next(),
                Height = Random.Next(),
                Data = Random.Next().ToString(),
            };
        }

        protected ReportedIssue GetRandomReportedIssue()
        {
            return new ReportedIssue
            {
                Id = Guid.NewGuid(),
                SpeakerId = Guid.NewGuid(),
                Browser = Random.Next().ToString(),
                Localization = Random.Next().ToString(),
                Message = Random.Next().ToString(),
                UserAgent = Random.Next().ToString(),
                IsImageAttached = Random.NextDouble() > 0.5,
            };
        }
    }
}