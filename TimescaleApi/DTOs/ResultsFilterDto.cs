namespace TimescaleApi.DTOs;

public class ResultsFilterDto
{
    public string? FileName { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public double? AvgValueFrom { get; set; }
    public double? AvgValueTo { get; set; }
    public double? AvgExecutionTimeFrom { get; set; }
    public double? AvgExecutionTimeTo { get; set; }
}