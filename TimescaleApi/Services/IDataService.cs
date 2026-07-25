using TimescaleApi.DTOs;
using TimescaleApi.Models;

namespace TimescaleApi.Services;

public interface IDataService
{

    Task SaveDataAsync(List<ValueRecord> records, ResultRecord statistics, CancellationToken cancellationToken);

    Task<List<ResultRecord>> GetResultAsync(ResultsFilterDto filter, CancellationToken cancellationToken);


    Task<List<ValueRecord>> GetLastTenValueAsync(string fileName, CancellationToken cancellationToken);
}