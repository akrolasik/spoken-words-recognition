using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Interfaces;

namespace Utils.UnitTests.Helpers
{
    public class DataRepositoryValidator<T> : IDataRepository<T> where T : ITableRow
    {
        private readonly IDataRepository<T> _dataRepository;

        public DataRepositoryValidator(IDataRepository<T> dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task AddRow(T row)
        {
            await _dataRepository.AddRow(row);
        }

        public async Task AddRows(IEnumerable<T> rows)
        {
            await _dataRepository.AddRows(rows);
        }

        public async Task<T> GetRow(Guid id)
        {
            return await _dataRepository.GetRow(id);
        }

        public async Task<List<T>> GetRows()
        {
            return await _dataRepository.GetRows();
        }

        public async Task DeleteRow(Guid id)
        {
            await _dataRepository.DeleteRow(id);
        }

        public async Task DeleteRows(IEnumerable<Guid> ids)
        {
            await _dataRepository.DeleteRows(ids);
        }
    }
}