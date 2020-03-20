using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using API.UnitTests.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using NUnit.Framework;
using Utils.Model;

namespace API.UnitTests.Tests
{
    public class AnalyticsControllerUnitTests : BaseControllerUnitTests
    {
        protected ReportedIssue ReportedIssue;
        protected Image Image;

        [OneTimeSetUp]
        public new void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            var token = Configuration["TestToken"];
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            ReportedIssue = GetRandomReportedIssue();
            Image = GetRandomImage();
        }

        [Test, Order(1)]
        public async Task PutIssue()
        {
            // Execution

            var result = await PutAsync("analytics/issue", ReportedIssue);

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var reportedIssue = await ReportedIssuesRepository.GetRow(ReportedIssue.Id);

            ReportedIssue.Timestamp = reportedIssue.Timestamp;
            AssertExtension.AreEqualJson(ReportedIssue, reportedIssue);
        }

        [Test, Order(2)]
        public async Task PutImage()
        {
            // Execution

            var result = await PutAsync($"analytics/issue/{ReportedIssue.Id}/image", Image);

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var image = await ImagesRepository.GetFileContent(ReportedIssue.Id);

            AssertExtension.AreEqualJson(Image, image);
        }

        [Test, Order(3)]
        public async Task GetIssues()
        {
            // Execution

            var result = await Client.GetAsync("analytics/issues");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var content = await result.Content.ReadAsStringAsync();
            var reportedIssue = JsonConvert.DeserializeObject<ReportedIssue[]>(content).FirstOrDefault();

            AssertExtension.AreEqualJson(ReportedIssue, reportedIssue);
        }

        [Test, Order(4)]
        public async Task GetImage()
        {
            // Execution

            var result = await Client.GetAsync($"analytics/issue/{ReportedIssue.Id}/image");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var content = await result.Content.ReadAsStringAsync();
            var image = JsonConvert.DeserializeObject<Image>(content);

            AssertExtension.AreEqualJson(Image, image);
        }

        [Test, Order(5)]
        public async Task DeleteIssue()
        {
            // Execution

            var result = await Client.DeleteAsync($"analytics/issue/{ReportedIssue.Id}");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var reportedIssue = await ReportedIssuesRepository.GetRow(ReportedIssue.Id);
            var image = await ImagesRepository.GetFileContent(ReportedIssue.Id);

            Assert.IsNull(reportedIssue);
            Assert.IsNull(image);
        }
    }

    [Order(2)]
    public class DataControllerUnitTests : BaseControllerUnitTests
    {
        protected Recording Recording;
        protected RawRecording RawRecording;
        protected RawFrequencies RawFrequencies;
        protected Frequencies Frequencies;

        private string _rawAudio;

        [OneTimeSetUp]
        public new void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            var token = Configuration["TestToken"];
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            Recording = GetRandomRecording();
            RawRecording = GetRandomRawRecording();
            RawFrequencies = GetRandomRawFrequencies();
            Frequencies = GetRandomFrequencies();
        }

        [Test, Order(1)]
        public async Task PutRawRecording()
        {
            // Execution

            var result = await PutAsync("data/raw", RawRecording);

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var rawRecording = await RawRecordingsRepository.GetRow(RawRecording.Id);

            RawRecording.Timestamp = rawRecording.Timestamp;
            AssertExtension.AreEqualJson(RawRecording, rawRecording);
        }

        [Test, Order(2)]
        public async Task PutRawAudio()
        {
            // Setup

            _rawAudio = Random.Next().ToString();

            // Execution

            var result = await PutAsync($"data/raw/{RawRecording.Id}/audio", _rawAudio);

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var rawAudioData = await AudioRepository.GetFileContent("raw-audio", $"{RawRecording.Id}.weba");

            AssertExtension.AreEqualJson(_rawAudio, rawAudioData);
        }

        [Test, Order(3)]
        public async Task PutRawFrequencies()
        {
            // Execution

            var result = await PutAsync($"data/raw/{RawRecording.Id}/frequencies", RawFrequencies);

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var rawFrequencies = await RawFrequenciesRepository.GetFileContent(RawRecording.Id);

            AssertExtension.AreEqualJson(RawFrequencies, rawFrequencies);
        }

        [Test, Order(4)]
        public async Task GetRawRecordings()
        {
            // Execution

            var result = await Client.GetAsync("data/raws");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var content = await result.Content.ReadAsStringAsync();
            var rawRecording = JsonConvert.DeserializeObject<RawRecording[]>(content).FirstOrDefault();

            AssertExtension.AreEqualJson(RawRecording, rawRecording);
        }

        [Test, Order(5)]
        public async Task GetRawAudioData()
        {
            // Execution

            var result = await Client.GetAsync($"data/raw/{RawRecording.Id}/audio");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var content = await result.Content.ReadAsStringAsync();

            AssertExtension.AreEqualJson(_rawAudio, content);
        }

        [Test, Order(6)]
        public async Task GetFrequencies()
        {
            // Execution

            var result = await Client.GetAsync($"data/raw/{RawRecording.Id}/frequencies");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var content = await result.Content.ReadAsStringAsync();
            var rawFrequencies = JsonConvert.DeserializeObject<RawFrequencies>(content);

            AssertExtension.AreEqualJson(RawFrequencies, rawFrequencies);
        }

        [Test, Order(7)]
        public async Task DeleteRawRecording()
        {
            // Execution

            var result = await Client.DeleteAsync($"data/raw/{RawRecording.Id}");

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var rawRecording = await RawRecordingsRepository.GetRow(RawRecording.Id);
            var rawAudio = await AudioRepository.GetFileContent("raw-audio", $"{RawRecording.Id}.weba");
            var rawFrequencies = await RawFrequenciesRepository.GetFileContent(RawRecording.Id);

            Assert.IsNull(rawRecording);
            Assert.IsNull(rawAudio);
            Assert.IsNull(rawFrequencies);
        }

        [Test, Order(8)]
        public async Task PutWords()
        {
            // Setup 

            var words = Enumerable.Range(0, 10).Select(x => GetRandomWordOccurence()).ToList();

            // Execution

            var result = await PutAsync($"data/{RawRecording.Id}/words", words);

            // Assertion

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var recordings = await RecordingsRepository.GetRows();

            Assert.NotNull(recordings);
            Assert.AreEqual(words.Count, recordings.Count);

            foreach (var recording in recordings)
            {
                //var mp3 = 
                throw new NotImplementedException();
            }
        }
    }
}