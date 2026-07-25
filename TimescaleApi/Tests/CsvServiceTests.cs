using System;
using System.Collections.Generic;
using TimescaleApi.Models;
using TimescaleApi.Services;
using Xunit;

namespace TimescaleApi.Tests;

public class CsvServiceTests
{
    private readonly CsvService _csvService = new();

    [Fact]
    public void CalculateStatistics_CalculatesCorrectly()
    {
        var records = new List<ValueRecord>
        {
            new() { Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 2.0, Value = 10.0 },
            new() { Date = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), ExecutionTime = 4.0, Value = 20.0 },
            new() { Date = new DateTime(2024, 1, 1, 11, 0, 0, DateTimeKind.Utc), ExecutionTime = 3.0, Value = 30.0 }
        };

        var result = _csvService.CalculateStatistics(records, "test_file");

        Assert.Equal("test_file", result.FileName);
        Assert.Equal(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), result.StartDate);
        Assert.Equal(7200.0, result.TimeDelta); 
        Assert.Equal(3.0, result.AvgExecutionTime);
        Assert.Equal(20.0, result.AvgValue);
        Assert.Equal(20.0, result.MedianValue); 
        Assert.Equal(30.0, result.MaxValue);
        Assert.Equal(10.0, result.MinValue);
    }

    [Fact]
    public void CalculateStatistics_EvenCount_CalculatesMedianCorrectly()
    {
        var records = new List<ValueRecord>
        {
            new() { Date = DateTime.UtcNow, ExecutionTime = 1, Value = 10.0 },
            new() { Date = DateTime.UtcNow, ExecutionTime = 1, Value = 20.0 },
            new() { Date = DateTime.UtcNow, ExecutionTime = 1, Value = 30.0 },
            new() { Date = DateTime.UtcNow, ExecutionTime = 1, Value = 40.0 }
        };

        var result = _csvService.CalculateStatistics(records, "test");

        Assert.Equal(25.0, result.MedianValue);
    }
}