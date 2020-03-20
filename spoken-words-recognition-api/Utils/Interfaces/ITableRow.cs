using System;

namespace Utils.Interfaces
{
    public interface ITableRow
    {
        Guid Id { get; set; }
        DateTimeOffset Timestamp { get; set; }
    }
}