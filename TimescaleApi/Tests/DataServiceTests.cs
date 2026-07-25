using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimescaleApi.Data;
using TimescaleApi.DTOs;
using TimescaleApi.Models;
using TimescaleApi.Services;
using Xunit;

namespace TimescaleApi.Tests;

public class DataServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DataService _dataService;

    public DataServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _dataService = new DataService(_context);
        
        SeedData();
    }

    private void SeedData()
    {
        var date1 = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc);
        var date2 = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);

        _context.Results.AddRange(
            new ResultRecord 
            { 
                Id = Guid.NewGuid(),
                FileName = "file1.csv", 
                StartDate = date1, 
                AvgValue = 50.0, 
                AvgExecutionTime = 2.0 
            },
            new ResultRecord 
            { 
                Id = Guid.NewGuid(),
                FileName = "file2.csv", 
                StartDate = date2, 
                AvgValue = 100.0, 
                AvgExecutionTime = 5.0 
            }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetResultAsync_WithFilters_ReturnsCorrectRecords()
    {
        var filter = new ResultsFilterDto { AvgValueFrom = 75.0 };

        var result = await _dataService.GetResultAsync(filter, default);

        Assert.Single(result);
        Assert.Equal("file2.csv", result[0].FileName);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}