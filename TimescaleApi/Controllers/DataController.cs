using Microsoft.AspNetCore.Mvc;
using TimescaleApi.Data;
using TimescaleApi.DTOs;
using TimescaleApi.Services;

namespace TimescaleApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController(ICsvService csvService, IDataService dataService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult<UploadResponseDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)   
            return BadRequest(new { error = "File is empty" });
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new  { error = "File is not a .csv file" });

        try
        {
            var (records, fileName) = await csvService.ParseAndValidateCsvAsync(file, cancellationToken);
            var statistics = csvService.CalculateStatistics(records, fileName);

            await dataService.SaveDataAsync(records, statistics, cancellationToken);

            return Ok(new UploadResponseDto
            {
                Message = "File has been uploaded",
                FileName = fileName,
                RecordCount = records.Count
            });
        }
        catch (FormatException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("results")]
    public async Task<ActionResult<IEnumerable<ResultRecordDto>>> Results([FromQuery] ResultsFilterDto filter, CancellationToken cancellationToken)
    {
        var records = await dataService.GetResultAsync(filter,cancellationToken);
        var dtos = records.Select(MapToResultDto);
        return Ok(dtos);
    }

    [HttpGet("values/{fileName}/last")]
    public async Task<ActionResult<IEnumerable<ValueRecordDto>>> Values([FromRoute] string fileName, CancellationToken cancellationToken)
    {
        var records = await dataService.GetLastTenValueAsync(fileName, cancellationToken);
        
        if (records is null || records.Count == 0)
            return NotFound(new { error = $"File '{fileName}' not found or empty"});

        var dtos = records.Select(MapToValueDto);
        return Ok(dtos);
    }

    private static ResultRecordDto MapToResultDto(Models.ResultRecord record) => new()
    {
        FileName = record.FileName,
        StartDate = record.StartDate,
        TimeDelta = record.TimeDelta,
        AvgExecutionTime = record.AvgExecutionTime,
        AvgValue = record.AvgValue,
        MedianValue = record.MedianValue,
        MaxValue = record.MaxValue,
        MinValue = record.MinValue
    };

    private static ValueRecordDto MapToValueDto(Models.ValueRecord record) => new()
    {
        Date = record.Date,
        ExecutionTime = record.ExecutionTime,
        Value = record.Value
    };
}