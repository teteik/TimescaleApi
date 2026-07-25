using System;
using TimescaleApi.Validators;
using Xunit;

namespace TimescaleApi.Tests;

public class CsvValidatorTests
{
    [Fact]
    public void ValidateAndParseLine_ValidLine_ParsesSuccessfully()
    {
        string line = "2024-01-15T10:30:45.1234Z;1.5;100.0";
        
        CsvValidator.ValidateAndParseLine(line, 1, out var date, out var execTime, out var value);
        
        Assert.Equal(DateTimeKind.Utc, date.Kind); 
        Assert.Equal(2024, date.Year);
        Assert.Equal(1, date.Month);
        Assert.Equal(15, date.Day);
        Assert.Equal(10, date.Hour);
        Assert.Equal(1.5, execTime);
        Assert.Equal(100.0, value);
    }

    [Theory]
    [InlineData("2024-01-15T10:30:45.1234Z;1.5", "exactly 3 values")]
    [InlineData("2024-01-15T10:30:45.1234Z;-1.5;100.0", "execution time must be a number >= 0")]
    [InlineData("2024-01-15T10:30:45.1234Z;1.5;-100.0", "value must be a number >= 0")]
    [InlineData("1999-01-15T10:30:45.1234Z;1.5;100.0", "01.01.2000")] 
    [InlineData("invalid-date;1.5;100.0", "invalid date format")]
    public void ValidateAndParseLine_InvalidLine_ThrowsException(string line, string expectedMessagePart)
    {
        var exception = Assert.ThrowsAny<Exception>(() => 
            CsvValidator.ValidateAndParseLine(line, 1, out _, out _, out _));
        
        Assert.Contains(expectedMessagePart, exception.Message);
    }

    [Fact]
    public void ValidateLineCount_TooManyLines_ThrowsException()
    {
        var exception = Assert.Throws<ArgumentException>(() => CsvValidator.ValidateLineCount(10001));
        Assert.Contains("between 1 and 10,000", exception.Message);
    }
}