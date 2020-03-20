using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils.Interfaces;

namespace Utils.Repositories
{
    public class CustomTableEntity : TableEntity, ITableRow
    {
        [JsonExtensionData]
        public Dictionary<string, JToken> Properties = new Dictionary<string, JToken>();

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            foreach (var property in properties)
            {
                if(property.Value.PropertyType == EdmType.String) Properties.Add(property.Key, JToken.FromObject(property.Value.StringValue));
                if(property.Value.PropertyType == EdmType.DateTime) Properties.Add(property.Key, JToken.FromObject(property.Value.DateTimeOffsetValue));
                if(property.Value.PropertyType == EdmType.Double) Properties.Add(property.Key, JToken.FromObject(property.Value.DoubleValue));
                if(property.Value.PropertyType == EdmType.Guid) Properties.Add(property.Key, JToken.FromObject(property.Value.GuidValue));
                if(property.Value.PropertyType == EdmType.Boolean) Properties.Add(property.Key, JToken.FromObject(property.Value.BooleanValue));
                if(property.Value.PropertyType == EdmType.Int32) Properties.Add(property.Key, JToken.FromObject(property.Value.Int32Value));
            }
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);

            foreach (var property in Properties)
            {
                var entityProperty = EntityProperty.GeneratePropertyForString(property.Value.ToString());

                if (property.Value.Type == JTokenType.Date) entityProperty = EntityProperty.GeneratePropertyForDateTimeOffset(property.Value.Value<DateTime>());
                if (property.Value.Type == JTokenType.Float) entityProperty = EntityProperty.GeneratePropertyForDouble(property.Value.Value<float>());
                if (property.Value.Type == JTokenType.Guid) entityProperty = EntityProperty.GeneratePropertyForGuid(property.Value.Value<Guid>());
                if (property.Value.Type == JTokenType.Boolean) entityProperty = EntityProperty.GeneratePropertyForBool(property.Value.Value<bool>());
                if (property.Value.Type == JTokenType.Integer) entityProperty = EntityProperty.GeneratePropertyForInt(property.Value.Value<int>());

                results.Add(property.Key, entityProperty);
            }

            return results;
        }

        [IgnoreProperty]
        public Guid Id
        {
            get => Guid.Parse(RowKey);
            set
            {
                RowKey = value.ToString();
                PartitionKey = string.Empty;
            }
        }
    }
}