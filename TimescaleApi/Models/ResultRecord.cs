namespace TimescaleApi.Models;

public class ResultRecord
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public double TimeDelta { get; set; }
    public double AvgExecutionTime { get; set; }
    public double AvgValue { get; set; }
    public double MedianValue { get; set; }
    public double MaxValue { get; set; }
    public double MinValue { get; set; }
}