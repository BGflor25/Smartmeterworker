using System.Text;
using SmartMeter.Worker.Parsing;

namespace SmartMeter.Tests;

public class Crc16Tests
{
    [Fact]
    public void Compute_matches_crc16_arc_check_value() =>
        Assert.Equal(0xBB3D, Crc16.Compute(Encoding.ASCII.GetBytes("123456789")));

    [Fact]
    public void IsValid_accepts_sample_telegram() =>
        Assert.True(Crc16.IsValid(Fixture.Read("telegram.txt")));

    [Fact]
    public void IsValid_rejects_corrupted_telegram()
    {
        var corrupted = Fixture.Read("telegram.txt").Replace("231.4*V", "231.5*V");
        Assert.False(Crc16.IsValid(corrupted));
    }

    [Fact]
    public void Framer_picks_only_the_complete_telegram_from_a_messy_capture()
    {
        var frames = TelegramFramer.Frames(Fixture.Read("capture.txt")).ToList();

        Assert.Single(frames);
        Assert.Equal(Fixture.Read("telegram.txt").TrimEnd(), TelegramFramer.LastValid(Fixture.Read("capture.txt")));
    }
}
