using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils.Interfaces
{
    public interface IDataRepository<T> where T : ITableRow
    {
        Task AddRow(T row);
        Task AddRows(IEnumerable<T> rows);
        Task<T> GetRow(Guid id);
        Task<List<T>> GetRows();
        Task DeleteRow(Guid id);
        Task DeleteRows(IEnumerable<Guid> ids);
    }
}