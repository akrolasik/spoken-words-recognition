using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using Newtonsoft.Json;
using Utils.Model;
using File = System.IO.File;

namespace NeuralNetwork.API.Data
{
    public class DataProvider
    {
        private static readonly Random Random = new Random();
        private const string DefaultDirectory = "C:/Users/plalkro/Downloads";

        public List<TrainingData> TrainingData;

        private readonly EvolutionConfig _evolutionConfig;
        private readonly Random _random = new Random();
        private readonly HttpClient _httpClient;

        public DataProvider(EvolutionConfig evolutionConfig)
        {
            _evolutionConfig = evolutionConfig;
            _httpClient = new HttpClient();

            if (!Directory.Exists("temp/recordings"))
            {
                Directory.CreateDirectory("temp/recordings");
            }

            using var archive = ZipFile.OpenRead($"{DefaultDirectory}/{evolutionConfig.InputConfig.PackageFileName}");
            var indexEntry = archive.Entries.First(x => x.Name == "index.tsv");

            using var stream = new StreamReader(indexEntry.Open(), Encoding.Default);
            var index = stream.ReadToEnd().Split('\n');
            var recordings = index.Select(x => (Recording)new Recording().FromText(x)).ToList();
            var words = recordings.Select(x => x.Word).Distinct().ToList();

            TrainingData = recordings.Select(recording =>
            {
                var outputIndex = words.IndexOf(recording.Word);
                return new TrainingData(this)
                {
                    OutputIndex = outputIndex,
                    ExpectedOutput = Matrix<float>.Build.Dense(words.Count, 1, (y, _) => outputIndex == y ? 1 : 0),
                    Recording = recording,
                };
            }).ToList();
        }

        public List<TrainingData> GetRandomDataSet()
        {
            var data = TrainingData.Select(x => x).ToList();

            var set = new List<TrainingData>();

            while (set.Count < _evolutionConfig.TrainingConfig.WordSetSize)
            {
                var index = _random.Next(data.Count);
                set.Add(data[index]);
                data.RemoveAt(index);
            }

            return set;
        }

        private async Task<FrequenciesChunk[]> GetFrequencies(Guid recordingId)
        {
            var path = $"temp/recordings/{recordingId}.json";
            string json;

            if (File.Exists(path))
            {
                json = await File.ReadAllTextAsync(path);
            }
            else
            {
                json = await LoadFrequencies(recordingId);
                await File.WriteAllTextAsync(path, json);
            }

            var frequencies = JsonConvert.DeserializeObject<FrequenciesChunk[]>(json);
            return frequencies.SkipWhile(x => x.Data.Length == 0).ToArray();
        }

        private async Task<string> LoadFrequencies(Guid recordingId)
        {
            var url = $"https://localhost:5001/data/{recordingId}/frequencies";
            var response = _httpClient.GetAsync(url);
            return await response.Result.Content.ReadAsStringAsync();
        }

        public async Task<Matrix<float>> GetInput(Recording recording)
        {
            var inputResolution = _evolutionConfig.NetworkConfig.InputResolution;
            var inputCount = _evolutionConfig.NetworkConfig.InputCount;

            var frequencies = await GetFrequencies(recording.Id);
            var frequenciesColumns = frequencies.First().Data.Length;
            var frequenciesRows = frequencies.Length;

            // var tsv = string.Join('\n', frequencies.Select(x => string.Join('\t', x.Data.Select(y => (int)y))));

            var blurred = Matrix<float>.Build.Dense(frequenciesRows, frequenciesColumns,
                (y, x) => Blur(frequencies, y, x, 1));

            // var blurTsv = string.Join('\n', Enumerable.Range(0, blurred.RowCount).Select(x => string.Join('\t', blurred.Row(x).Select(y => y))));

            var downsized = Matrix<float>.Build.Dense(inputResolution.Width, inputResolution.Height,
                (y, x) => Downsize(blurred, frequenciesRows, frequenciesColumns, inputResolution.Width, inputResolution.Height, y, x));

            // var downsizedTsv = string.Join('\n', Enumerable.Range(0, downsized.RowCount).Select(x => string.Join('\t', downsized.Row(x).Select(y => y))));

            var result = Matrix<float>.Build.Dense(inputCount, 1, (i, _) =>
            {
                var y = i / inputResolution.Height;
                var x = i % inputResolution.Height;
                return downsized[y, x]; // GetRandom();
            });

            return result;
        }

        private float GetRandom()
        {
            lock (Random)
            {
                return (float)(Random.NextDouble() * 2 - 1);
            }
        }

        private float Downsize(Matrix<float> input, int inputHeight, int inputWidth, int outputHeight, int outputWidth, int y, int x)
        {
            var ry = (float)inputHeight / outputHeight;
            var rx = (float)inputWidth / outputWidth;

            var ty = (float)y / outputHeight * inputHeight;
            var tx = (float)x / outputWidth * inputWidth;

            var sum = 0.0;
            var weight = 0.0;

            for (var fy = 0; fy < inputHeight; fy++)
            {
                for (var fx = 0; fx < inputWidth; fx++)
                {
                    if (fy + 1 >= ty - ry && fy <= ty + ry &&
                        fx + 1 >= tx - rx && fx <= tx + rx)
                    {
                        var wy0 = Math.Max(ty - ry, fy);
                        var wy1 = Math.Min(ty + ry, fy + 1);

                        var wx0 = Math.Max(tx - rx, fx);
                        var wx1 = Math.Min(tx + rx, fx + 1);

                        var w = (wy1 - wy0) * (wx1 - wx0);

                        weight += w;
                        sum += input[fy, fx] * w;
                    }
                }
            }

            return (float)(sum / weight);
        }

        private float Blur(FrequenciesChunk[] frequencies, int y0, int x0, int halfSize)
        {
            var sum = 0.0;
            var weight = 0.0;

            for (var y = y0 - halfSize; y <= y0 + halfSize; y++)
            {
                for (var x = x0 - halfSize; x <= x0 + halfSize; x++)
                {
                    if (y >= 0 && y < frequencies.Length && x >= 0 && x < frequencies[y].Data.Length)
                    {
                        var w = 1.0 / (1.0 + Math.Abs(x - x0)) / (1.0 + Math.Abs(y - y0));
                        sum += frequencies[y].Data[x] / 256.0 * w;
                        weight += w;
                    }
                }
            }

            return (float)(sum / weight);
        }
    }
}