using System.Globalization;

namespace TimescaleApi.Validators;

public static class CsvValidator
{
    private static readonly DateTime MinDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.ffffZ";

    public static void ValidateLineCount(int count)
    {
        if (count is < 1 or > 10000)
            throw new ArgumentException($"Line count must be between 1 and 10,000. Current: {count}");
    }

    public static void ValidateAndParseLine(string line, int lineNumber, out DateTime date, out double executionTime, out double value)
    {
        var parts = line.Split(';');
        if (parts.Length != 3)
            throw new FormatException($"Line {lineNumber}: exactly 3 values separated by ';' are expected.");

        if (!DateTime.TryParseExact(parts[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out date))
            throw new FormatException($"Line {lineNumber}: invalid date format '{parts[0]}'. Expected format: {DateFormat}");

        if (date > DateTime.UtcNow || date < MinDate)
            throw new ArgumentOutOfRangeException($"Line {lineNumber}: date {date:yyyy-MM-dd} must be between 01.01.2000 and the current UTC time.");

        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out executionTime) || executionTime < 0)
            throw new FormatException($"Line {lineNumber}: execution time must be a number >= 0.");

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out value) || value < 0)
            throw new FormatException($"Line {lineNumber}: value must be a number >= 0.");
    }
}