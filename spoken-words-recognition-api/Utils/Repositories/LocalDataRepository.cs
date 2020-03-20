using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Configuration;
using Utils.Interfaces;

namespace Utils.Repositories
{
    public class LocalDataRepository<T> : IDataRepository<T> where T : class, ITableRow
    {
        public LocalDataRepository(IRepositoryConfig repositoryConfig) { }

        public Dictionary<Guid, ITableRow> Rows = new Dictionary<Guid, ITableRow>();

        public Task AddRow(T row)
        {
            row.Timestamp = DateTimeOffset.UtcNow;
            Rows[row.Id] = row;
            return Task.CompletedTask;
        }

        public Task AddRows(IEnumerable<T> rows)
        {
            foreach (var tableRow in rows)
            {
                tableRow.Timestamp = DateTimeOffset.UtcNow;
                Rows[tableRow.Id] = tableRow;
            }

            return Task.CompletedTask;
        }

        public Task<T> GetRow(Guid id)
        {
            Rows.TryGetValue(id, out var value);
            return Task.FromResult((T)value);
        }

        public Task<List<T>> GetRows()
        {
            return Task.FromResult(Rows.Select(x => x.Value).Cast<T>().ToList());
        }

        public Task DeleteRow(Guid id)
        {
            if (Rows.ContainsKey(id))
                Rows.Remove(id);

            return Task.CompletedTask;
        }

        public Task DeleteRows(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
                if (Rows.ContainsKey(id))
                    Rows.Remove(id);

            return Task.CompletedTask;
        }
    }
}