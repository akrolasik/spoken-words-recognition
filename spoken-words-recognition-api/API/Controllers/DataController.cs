using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utils.Interfaces;
using Utils.Model;

namespace API.Controllers
{
    [ApiController]
    [Route("data")]
    [Authorize(Policy = "ClaimsAuthorizationPolicy")]
    public class DataController : ControllerBase
    {
        private readonly IDataRepository<RawRecording> _rawRecordingsRepository;
        private readonly IDataRepository<Recording> _recordingsRepository;
        private readonly IFileRepository<RawFrequencies> _rawFrequenciesRepository;
        private readonly IFileRepository<Frequencies> _frequenciesRepository;
        private readonly IFileRepository _audioRepository;

        private static readonly Dictionary<Guid, List<FrequenciesChunk>> FrequenciesChunk =  new Dictionary<Guid, List<FrequenciesChunk>>();

        public DataController(
            IDataRepository<RawRecording> rawRecordingsRepository, 
            IDataRepository<Recording> recordingsRepository,
            IFileRepository<RawFrequencies> rawFrequenciesRepository,
            IFileRepository<Frequencies> frequenciesRepository,
            IFileRepository audioRepository)
            {
                _rawRecordingsRepository = rawRecordingsRepository;
                _recordingsRepository = recordingsRepository;
                _rawFrequenciesRepository = rawFrequenciesRepository;
                _frequenciesRepository = frequenciesRepository;
                _audioRepository = audioRepository;
            }

        [HttpPut]
        [Route("raw")]
        [AllowAnonymous]
        public async Task<ActionResult> PutRawRecording([FromBody]RawRecording rawRecording)
        {
            rawRecording.Words = rawRecording.Words.OrderBy(x => x.Word.Value.Length == 1 ? $"0{x.Word.Value}" : x.Word.Value).ToArray();
            await _rawRecordingsRepository.AddRow(rawRecording);
            return Ok();
        }

        [HttpPut]
        [Route("raw/{rawRecordingId}/audio")]
        [AllowAnonymous]
        public async Task<ActionResult> PutRawAudio([FromRoute]Guid rawRecordingId)
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var audio = await reader.ReadToEndAsync();

            var fileName = $"{rawRecordingId}.weba";

            var ansiWriter = new StreamWriter(fileName, false, Encoding.GetEncoding(1250));
            ansiWriter.Write(CharArrayToAnsiString(audio));
            ansiWriter.Close();

            await _audioRepository.UploadFile("raw-audio", fileName, fileName);

            System.IO.File.Delete(fileName);

            return Ok();
        }

        // Should be a stream but I couldn't make it work
        [HttpPut]
        [Route("raw/{rawRecordingId}/frequencies")]
        [AllowAnonymous]
        public ActionResult PutRawFrequencies([FromRoute]Guid rawRecordingId, [FromBody]FrequenciesChunk[] frequenciesChunks, [FromQuery] bool end = false)
        {
            if (!FrequenciesChunk.ContainsKey(rawRecordingId))
            {
                FrequenciesChunk[rawRecordingId] = new List<FrequenciesChunk>();
            }

            FrequenciesChunk[rawRecordingId].AddRange(frequenciesChunks);

            if (end)
            {
                _rawFrequenciesRepository.SetFileContent(rawRecordingId, new RawFrequencies
                {
                    FrequenciesChunks = FrequenciesChunk[rawRecordingId].ToArray()
                });

                FrequenciesChunk.Remove(rawRecordingId);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("raw/{rawRecordingId}")]
        public async Task<ActionResult> DeleteRawRecording([FromRoute]Guid rawRecordingId)
        {
            await _rawRecordingsRepository.DeleteRow(rawRecordingId);
            await _rawFrequenciesRepository.DeleteFile(rawRecordingId);
            await _audioRepository.DeleteFile("raw-audio", $"{rawRecordingId}.weba");
            return Ok();
        }

        [HttpGet]
        [Route("raws")]
        public async Task<ActionResult<List<RawRecording>>> GetRawRecordings()
        {
            return await _rawRecordingsRepository.GetRows();
        }

        [HttpGet]
        [Route("raw/{rawRecordingId}/audio")]
        public async Task<ActionResult<string>> GetAudioChunks([FromRoute]Guid rawRecordingId)
        {
            var fileName = $"{rawRecordingId}.weba";
            await _audioRepository.DownloadFile("raw-audio", fileName, fileName);

            var data = System.IO.File.ReadAllBytes(fileName);

            System.IO.File.Delete(fileName);

            return File(data, "audio/webm");
        }

        [HttpGet]
        [Route("raw/{rawRecordingId}/frequencies")]
        public async Task<ActionResult<FrequenciesChunk[]>> GetRawFrequencies([FromRoute]Guid rawRecordingId)
        {
            return (await _rawFrequenciesRepository.GetFileContent(rawRecordingId)).FrequenciesChunks;
        }

        [HttpPut]
        [Route("raw/{rawRecordingId}/words")]
        public async Task<ActionResult> PutWords([FromRoute]Guid rawRecordingId, [FromBody]WordOccurence[] words)
        {
            var rawRecording = await _rawRecordingsRepository.GetRow(rawRecordingId);

            var recordings = words.ToDictionary(x => x, x => new Recording
            {
                Id = Guid.NewGuid(),
                SpeakerId = rawRecording.SpeakerId,
                Accent = rawRecording.Accent,
                Modification = rawRecording.Modification,
                Localization = rawRecording.Localization,
                Word = x.Word.InWords,
                NotGoodForTraining = x.NotGoodForTraining
            }).ToList();

            var fileName = $"{rawRecordingId}.weba";
            await _audioRepository.DownloadFile("raw-audio", fileName, fileName);
            var rawFrequencies = await _rawFrequenciesRepository.GetFileContent(rawRecordingId);

            foreach (var (word, recording) in recordings)
            {
                CreateMp3(rawRecordingId, recording.Id, word);
                await UploadMp3(recording.Id);
                System.IO.File.Delete($"{recording.Id}.mp3");

                recording.ChunksCount = await UploadFrequencies(rawFrequencies, recording.Id, word);
            }

            await _recordingsRepository.AddRows(recordings.Select(x => x.Value));

            System.IO.File.Delete(fileName);

            _ = DeleteRawRecording(rawRecordingId);

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{recordingId}/frequencies")]
        public async Task<ActionResult<FrequenciesChunk[]>> GetFrequencies([FromRoute]Guid recordingId)
        {
            return (await _frequenciesRepository.GetFileContent(recordingId)).FrequenciesChunks;
        }

        private void CreateMp3(Guid rawRecordingId, Guid recordingId, WordOccurence word)
        {
            var margin = 0.5f;
            var ss = (word.Start / 1000.0f - margin).Value.ToString(CultureInfo.InvariantCulture);
            var t = ((word.End - word.Start) / 1000.0f + margin * 2).Value.ToString(CultureInfo.InvariantCulture);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i {rawRecordingId}.weba -ss {ss} -t {t} {recordingId}.mp3"
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private async Task UploadMp3(Guid recordingId)
        {
            var filename = $"{recordingId}.mp3";
            await _audioRepository.UploadFile("audio", filename, filename);
        }

        private async Task<int> UploadFrequencies(RawFrequencies rawFrequencies, Guid recordingId, WordOccurence word)
        {
            var frequencies = new Frequencies
            {
                FrequenciesChunks = rawFrequencies.FrequenciesChunks.Where(x =>
                    x.Milliseconds >= word.Start && 
                    x.Milliseconds <= word.End).ToArray()
            };

            await _frequenciesRepository.SetFileContent(recordingId, frequencies);

            return frequencies.FrequenciesChunks.Length;
        }

        private string CharArrayToAnsiString(string array)
        {
            return Encoding.GetEncoding(1250).GetString(array.Select(x => (byte)x).ToArray());
        }
    }
}
