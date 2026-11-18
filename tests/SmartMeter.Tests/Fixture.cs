using System.Text;

namespace SmartMeter.Tests;

internal static class Fixture
{
    public static string Read(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name), Encoding.Latin1);
}
