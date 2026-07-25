using TimescaleApi.Models;

namespace TimescaleApi.Services;

public interface ICsvService
{
    Task<(List<ValueRecord> records, string fileName)> ParseAndValidateCsvAsync(IFormFile fileName, CancellationToken cancellationToken);
    
    ResultRecord CalculateStatistics(List<ValueRecord> records, string fileName);
}