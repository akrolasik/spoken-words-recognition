using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Utils.Configuration;
using Utils.Interfaces;
using Utils.Repositories;
using Utils.UnitTests.Helpers;

namespace Utils.UnitTests.Tests
{
    [TestFixture(typeof(AzureTableStorage<ExampleRow>))]
    [TestFixture(typeof(LocalDataRepository<ExampleRow>))]
    public class DataRepositoryUnitTests<T>
    {
        private readonly Random _random = new Random();

        private IDataRepository<ExampleRow> _dataRepository;
        private DataRepositoryValidator<ExampleRow> _dataRepositoryValidator;
        private List<ExampleRow> _exampleRows;

        [OneTimeSetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();

            var azureStorageConfig = new AzureStorageConfig();
            configuration.Bind("AzureStorage", azureStorageConfig);

            _dataRepository = (IDataRepository<ExampleRow>)Activator.CreateInstance(typeof(T), azureStorageConfig);
            _dataRepositoryValidator = new DataRepositoryValidator<ExampleRow>(_dataRepository);
            _exampleRows = new List<ExampleRow>();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            var rows = _dataRepositoryValidator.GetRows().Result;
            _dataRepositoryValidator.DeleteRows(rows.Select(x => x.Id)).Wait();
        }

        [Test, Order(1)]
        public async Task AddRow()
        {
            // Setup

            var row = GetNewExampleRow();
            _exampleRows.Add(row);

            // Execution

            await _dataRepository.AddRow(row);

            // Assertion

            var result = await _dataRepositoryValidator.GetRow(row.Id);

            row.Timestamp = result.Timestamp;
            AssertExtension.AreEqualJson(row, result);
        }

        [Test, Order(2)]
        public async Task UpdateRow()
        {
            // Setup

            var id = _exampleRows.First().Id;
            _exampleRows.Clear();

            var row = GetNewExampleRow();
            row.Id = id;

            _exampleRows.Add(row);

            // Execution

            await _dataRepository.AddRow(row);

            // Assertion

            var result = await _dataRepositoryValidator.GetRow(row.Id);

            row.Timestamp = result.Timestamp;
            AssertExtension.AreEqualJson(row, result);
        }

        [Test, Order(3)]
        public async Task GetRow()
        {
            // Setup

            var row = _exampleRows.First();

            // Execution

            var result = await _dataRepository.GetRow(row.Id);

            // Assertion

            row.Timestamp = result.Timestamp;
            AssertExtension.AreEqualJson(row, result);
        }

        [Test, Order(4)]
        public async Task DeleteRow()
        {
            // Setup

            var row = _exampleRows.First();

            // Execution

            await _dataRepository.DeleteRow(row.Id);

            // Assertion

            var result = await _dataRepositoryValidator.GetRow(row.Id);

            Assert.IsNull(result);
        }

        [Test, Order(5)]
        public async Task DeleteNonExistingRow()
        {
            // Setup

            var row = _exampleRows.First();

            // Execution

            await _dataRepository.DeleteRow(row.Id);
        }

        [Test, Order(6)]
        public async Task GetNonExistingRow()
        {
            // Setup

            var row = _exampleRows.First();

            // Execution

            await _dataRepository.GetRow(row.Id);

            // Cleanup

            _exampleRows.Clear();
        }

        [Test, Order(7)]
        public async Task AddRows()
        {
            // Setup

            var rows = Enumerable.Range(0, 10).Select(x => GetNewExampleRow()).ToList();
            _exampleRows.AddRange(rows);

            // Execution

            await _dataRepository.AddRows(rows);

            // Assertion

            var result = await _dataRepositoryValidator.GetRows();

            foreach (var expected in rows)
            {
                var actual = result.First(x => x.Id == expected.Id);
                expected.Timestamp = actual.Timestamp;
                AssertExtension.AreEqualJson(expected, actual);
            }
        }

        [Test, Order(8)]
        public async Task UpdateRows()
        {
            // Setup

            var ids = _exampleRows.Select(x => x.Id).ToList();
            _exampleRows.Clear();

            var rows = ids.Select(x =>
            {
                var row = GetNewExampleRow();
                row.Id = x;
                return row;
            }).ToList();

            _exampleRows.AddRange(rows);

            // Execution

            await _dataRepository.AddRows(rows);

            // Assertion

            var result = await _dataRepositoryValidator.GetRows();

            foreach (var expected in rows)
            {
                var actual = result.First(x => x.Id == expected.Id);
                expected.Timestamp = actual.Timestamp;
                AssertExtension.AreEqualJson(expected, actual);
            }
        }

        [Test, Order(9)]
        public async Task GetRows()
        {
            // Execution

            var result = await _dataRepository.GetRows();

            // Assertion

            foreach (var expected in _exampleRows)
            {
                var actual = result.First(x => x.Id == expected.Id);
                expected.Timestamp = actual.Timestamp;
                AssertExtension.AreEqualJson(expected, actual);
            }
        }

        [Test, Order(10)]
        public async Task DeleteRows()
        {
            // Execution

            await _dataRepository.DeleteRows(_exampleRows.Select(x => x.Id));

            // Assertion

            var result = await _dataRepositoryValidator.GetRows();
            Assert.IsEmpty(result);
        }

        [Test, Order(11)]
        public async Task DeleteNonExistingRows()
        {
            // Execution

            await _dataRepository.DeleteRows(_exampleRows.Select(x => x.Id));
        }

        [Test, Order(12)]
        public async Task GetNonExistingRows()
        {
            // Execution

            await _dataRepository.GetRows();

            // Cleanup

            _exampleRows.Clear();
        }

        private ExampleRow GetNewExampleRow()
        {
            return new ExampleRow
            {
                Id = Guid.NewGuid(),
                String = _random.Next().ToString(),
                Integer = _random.Next(),
                Boolean = _random.NextDouble() > 0.5,
                Float = (float)_random.NextDouble(),
                Guid = Guid.NewGuid(),
                DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(_random.Next()),
                TimeSpan = TimeSpan.FromSeconds(_random.Next()),
                Enum = (ExampleEnum)_random.Next(3),
                ExampleArray = Enumerable.Range(0, 10).Select(x => new ExampleArrayElement
                {
                    String = _random.Next().ToString(),
                    Integer = x
                }).ToArray()
            };
        }
    }
}