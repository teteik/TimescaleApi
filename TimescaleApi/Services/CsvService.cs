using System.Globalization;
using Microsoft.AspNetCore.Http;
using TimescaleApi.Models;
using TimescaleApi.Validators;

namespace TimescaleApi.Services;

public class CsvService : ICsvService
{
    private const string ExpectedHeader = "Date;ExecutionTime;Value";
    private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.ffffZ";

    public async Task<(List<ValueRecord> records, string fileName)> ParseAndValidateCsvAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        var records = new List<ValueRecord>();
        var fileName = Path.GetFileNameWithoutExtension(file.FileName);

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var header = await reader.ReadLineAsync(cancellationToken);
        if (header != ExpectedHeader)
            throw new FormatException($"Invalid CSV header. Expected: {ExpectedHeader}");

        var lineNumber = 1;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            CsvValidator.ValidateAndParseLine(line, lineNumber, out var date, out var execTime, out var value);

            records.Add(new ValueRecord
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Date = date,
                ExecutionTime = execTime,
                Value = value
            });
        }

        CsvValidator.ValidateLineCount(records.Count);

        return (records, fileName);
    }

    public ResultRecord CalculateStatistics(List<ValueRecord> records, string fileName)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        if (records.Count == 0)
            throw new InvalidOperationException("Cannot calculate statistics for an empty list");

        var minDate = records[0].Date;
        var maxDate = records[0].Date;
        var minValue = records[0].Value;
        var maxValue = records[0].Value;
        var sumExecutionTime = 0.0;
        var sumValue = 0.0;

        foreach (var record in records)
        {
            if (record.Date < minDate) minDate = record.Date;
            if (record.Date > maxDate) maxDate = record.Date;
            if (record.Value < minValue) minValue = record.Value;
            if (record.Value > maxValue) maxValue = record.Value;

            sumExecutionTime += record.ExecutionTime;
            sumValue += record.Value;
        }

        var count = records.Count;
        var avgExecutionTime = sumExecutionTime / count;
        var avgValue = sumValue / count;
        var median = CalculateMedian(records);

        return new ResultRecord
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            StartDate = minDate,
            TimeDelta = (maxDate - minDate).TotalSeconds,
            AvgExecutionTime = avgExecutionTime,
            AvgValue = avgValue,
            MedianValue = median,
            MaxValue = maxValue,
            MinValue = minValue
        };
    }

    private static double CalculateMedian(List<ValueRecord> records)
    {
        var sortedValues = records.Select(r => r.Value).OrderBy(v => v).ToList();
        var count = sortedValues.Count;

        if (count % 2 == 0)
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;

        return sortedValues[count / 2];
    }
}