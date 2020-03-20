using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils.Configuration;
using Utils.Extensions;
using Utils.Interfaces;

namespace Utils.Repositories
{
    public class ArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JsonConvert.DeserializeObject(JToken.Load(reader).ToString(), objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsArray;
        }
    }

    public class AzureTableStorage<T> : IDataRepository<T> where T : ITableRow
    {
        private const string RowKey = "RowKey";
        private const string Timestamp = "Timestamp";

        private readonly CloudTable _cloudTable;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public AzureTableStorage(IRepositoryConfig repositoryConfig)
        {
            if (!(repositoryConfig is AzureStorageConfig azureStorageConfig))
            {
                throw new ArgumentNullException("Missing azureStorageConfig");
            }

            azureStorageConfig.AssertAllPropertiesNotNull();

            var storageAccount = CloudStorageAccount.Parse(azureStorageConfig.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _cloudTable = tableClient.GetTableReference(typeof(T).Name);
            _cloudTable.CreateIfNotExistsAsync().Wait();

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new ArrayConverter()
                }
            };
        }

        public async Task AddRow(T row)
        {
            var json = JsonConvert.SerializeObject(row, _jsonSerializerSettings);
            var entity = JsonConvert.DeserializeObject<CustomTableEntity>(json);

            var tableOperation = TableOperation.InsertOrReplace(entity);
            await _cloudTable.ExecuteAsync(tableOperation);
        }

        public async Task AddRows(IEnumerable<T> rows)
        {
            var batchOperation = new TableBatchOperation();

            foreach (var row in rows)
            {
                var json = JsonConvert.SerializeObject(row, _jsonSerializerSettings);
                var entity = JsonConvert.DeserializeObject<CustomTableEntity>(json);
                batchOperation.InsertOrReplace(entity);
            }

            await _cloudTable.ExecuteBatchAsync(batchOperation);
        }

        public async Task<T> GetRow(Guid id)
        {
            var query = new TableQuery<CustomTableEntity>
            {
                FilterString = TableQuery.GenerateFilterCondition(RowKey, QueryComparisons.Equal, id.ToString())
            };

            var entities = await _cloudTable.ExecuteQuerySegmentedAsync(query, null);
            var json = JsonConvert.SerializeObject(entities.FirstOrDefault());

            return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
        }

        public async Task<List<T>> GetRows()
        {
            var query = new TableQuery<CustomTableEntity>();

            TableContinuationToken continuationToken = null;
            var customTableEntities = new List<CustomTableEntity>();

            do
            {
                var querySegment = await _cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = querySegment.ContinuationToken;
                customTableEntities.AddRange(querySegment.Results);
            }
            while (continuationToken != null);

            return customTableEntities.Select(entity =>
            {
                var json = JsonConvert.SerializeObject(entity);
                return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
            }).ToList();
        }

        public async Task DeleteRow(Guid id)
        {
            var query = new TableQuery<CustomTableEntity>
            {
                FilterString = TableQuery.GenerateFilterCondition(RowKey, QueryComparisons.Equal, id.ToString())
            };

            var querySegment = await _cloudTable.ExecuteQuerySegmentedAsync(query, null);
            var rawData = querySegment.Results.FirstOrDefault();

            if (rawData != null)
            {
                var delete = TableOperation.Delete(rawData);
                await _cloudTable.ExecuteAsync(delete);
            }
        }

        public async Task DeleteRows(IEnumerable<Guid> ids)
        {
            var query = new TableQuery<CustomTableEntity>
            {
                FilterString = string.Join(" or ", ids.Select(id => $"({RowKey} eq '{id}')"))
            };

            TableContinuationToken continuationToken = null;
            var customTableEntities = new List<CustomTableEntity>();

            do
            {
                var querySegment = await _cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = querySegment.ContinuationToken;
                customTableEntities.AddRange(querySegment.Results);
            }
            while (continuationToken != null);

            var tableBatchOperation = new TableBatchOperation();

            if (customTableEntities.Any())
            {
                customTableEntities.ForEach(x => tableBatchOperation.Add(TableOperation.Delete(x)));
                await _cloudTable.ExecuteBatchAsync(tableBatchOperation);
            }
        }
    }
}
