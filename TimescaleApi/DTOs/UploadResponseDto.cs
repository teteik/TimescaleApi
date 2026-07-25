namespace TimescaleApi.DTOs;

public class UploadResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int RecordCount { get; set; }
}