using SmartMeter.Worker.Parsing;

namespace SmartMeter.Tests;

public class DsmrParserTests
{
    private readonly Telegram _t = DsmrParser.Parse(Fixture.Read("telegram.txt"));

    [Fact]
    public void Parses_header_and_checksum()
    {
        Assert.Equal(@"/FLU5\253769484_A", _t.Header);
        Assert.Equal("72BC", _t.Checksum);
    }

    [Theory]
    [InlineData("1-0:1.8.1", 4567.123)]
    [InlineData("1-0:1.7.0", 0.456)]
    [InlineData("1-0:32.7.0", 231.4)]
    [InlineData("0-0:96.14.0", 1)]
    [InlineData("1-0:1.6.0", 4.321)]      // multi-group: timestamp + value
    [InlineData("0-1:24.2.3", 2345.678)]  // gas: timestamp + value
    [InlineData("1-0:31.4.0", 999)]
    [InlineData("0-0:96.3.10", 1)]
    public void Parses_numbers_using_last_group(string obis, double expected) =>
        Assert.Equal(expected, _t.GetNumber(obis));

    [Fact]
    public void Missing_or_non_numeric_values_are_null()
    {
        Assert.Null(_t.GetNumber("1-0:41.7.0"));
        Assert.Null(_t.GetNumber("0-0:96.13.0"));
        Assert.Null(_t.GetNumber("0-0:1.0.0"));
    }

    [Fact]
    public void Decodes_hex_serials()
    {
        Assert.Equal("1SAG1100484224", _t.GetHexText("0-0:96.1.1"));
        Assert.Equal("7FLO2119033733", _t.GetHexText("0-1:96.1.1"));
    }

    [Theory]
    [InlineData("261118093015W", "2026-11-18T09:30:15+01:00")]
    [InlineData("260906173151S", "2026-09-06T17:31:51+02:00")]
    public void Parses_timestamps(string raw, string expected) =>
        Assert.Equal(DateTimeOffset.Parse(expected), DsmrTimestamp.Parse(raw));

    [Theory]
    [InlineData("garbage")]
    [InlineData("261340093015W")]
    [InlineData(null)]
    public void Invalid_timestamps_are_null(string? raw) =>
        Assert.Null(DsmrTimestamp.Parse(raw));
}
