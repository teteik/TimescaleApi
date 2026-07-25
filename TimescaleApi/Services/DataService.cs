using Microsoft.EntityFrameworkCore;
using TimescaleApi.Data;
using TimescaleApi.DTOs;
using TimescaleApi.Models;

namespace TimescaleApi.Services;

public class DataService(AppDbContext context) : IDataService
{
    public async Task SaveDataAsync(
        List<ValueRecord> records,
        ResultRecord statistics,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(statistics);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var fileName = statistics.FileName;

            await context.Values
                .Where(v => v.FileName == fileName)
                .ExecuteDeleteAsync(cancellationToken);

            await context.Results
                .Where(r => r.FileName == fileName)
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var record in records)
            {
                if (record.Id == Guid.Empty)
                    record.Id = Guid.NewGuid();
            }

            if (statistics.Id == Guid.Empty)
                statistics.Id = Guid.NewGuid();

            await context.Values.AddRangeAsync(records, cancellationToken);
            await context.Results.AddAsync(statistics, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<List<ResultRecord>> GetResultAsync(
        ResultsFilterDto filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = context.Results.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.FileName))
            query = query.Where(r => r.FileName == filter.FileName);

        if (filter.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= filter.StartDateFrom.Value);

        if (filter.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= filter.StartDateTo.Value);

        if (filter.AvgValueFrom.HasValue)
            query = query.Where(r => r.AvgValue >= filter.AvgValueFrom.Value);

        if (filter.AvgValueTo.HasValue)
            query = query.Where(r => r.AvgValue <= filter.AvgValueTo.Value);

        if (filter.AvgExecutionTimeFrom.HasValue)
            query = query.Where(r => r.AvgExecutionTime >= filter.AvgExecutionTimeFrom.Value);

        if (filter.AvgExecutionTimeTo.HasValue)
            query = query.Where(r => r.AvgExecutionTime <= filter.AvgExecutionTimeTo.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<ValueRecord>> GetLastTenValueAsync(
        string fileName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        return await context.Values
            .AsNoTracking()
            .Where(v => v.FileName == fileName)
            .OrderByDescending(v => v.Date)
            .Take(10)
            .ToListAsync(cancellationToken);
    }
}