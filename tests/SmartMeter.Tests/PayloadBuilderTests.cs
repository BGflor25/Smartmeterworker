using System.Text.Json;
using System.Text.Json.Nodes;
using SmartMeter.Worker.Api;
using SmartMeter.Worker.Parsing;

namespace SmartMeter.Tests;

public class PayloadBuilderTests
{
    // Keys the old Python script hard-coded to 0 that are now filled from the telegram.
    private static readonly Dictionary<string, JsonNode> NewlyFilled = new()
    {
        ["1-0:1.4.0"] = JsonNode.Parse("""{"value":1.234,"unit":"kW"}""")!,
        ["1-0:1.6.0"] = JsonValue.Create(4.321),
        ["0-0:96.3.10"] = JsonValue.Create(1),
        ["0-0:17.0.0"] = JsonNode.Parse("""{"value":999.9,"unit":"kW"}""")!,
        ["max_current"] = JsonNode.Parse("""{"value":999,"unit":"A"}""")!,
        ["0-1:24.4.0"] = JsonValue.Create(1),
    };

    private static JsonObject Actual() =>
        JsonSerializer.SerializeToNode(PayloadBuilder.Build(DsmrParser.Parse(Fixture.Read("telegram.txt"))))!.AsObject();

    private static JsonObject Python() =>
        JsonNode.Parse(Fixture.Read("python-output.json"))!.AsObject();

    [Fact]
    public void Has_exactly_the_same_keys_in_the_same_order_as_the_python_output() =>
        Assert.Equal(Python().Select(p => p.Key), Actual().Select(p => p.Key));

    [Fact]
    public void Matches_python_output_except_newly_filled_fields()
    {
        var actual = Actual();
        foreach (var (key, expected) in Python())
        {
            var value = NewlyFilled.TryGetValue(key, out var filled) ? filled : expected;
            Assert.True(JsonNode.DeepEquals(Normalize(value), Normalize(actual[key])),
                $"{key}: expected {value?.ToJsonString()}, got {actual[key]?.ToJsonString()}");
        }
    }

    [Fact]
    public void Empty_telegram_defaults_everything_to_zero()
    {
        var payload = PayloadBuilder.Build(DsmrParser.Parse("/X\r\n!"));
        Assert.Equal(0, payload.Timestamp);
        Assert.Equal(new ValueUnit(0, "kW"), payload.CurrentPowerUsage);
        Assert.Equal("", payload.Checksum);
    }

    // Python writes 3.0 / 0 while System.Text.Json writes 3 / 0; compare numerically.
    private static JsonNode? Normalize(JsonNode? node) => node switch
    {
        JsonValue v when v.GetValueKind() == JsonValueKind.Number => JsonValue.Create(double.Parse(v.ToJsonString(), System.Globalization.CultureInfo.InvariantCulture)),
        JsonObject o => new JsonObject(o.Select(p => KeyValuePair.Create(p.Key, Normalize(p.Value)))),
        _ => node?.DeepClone(),
    };
}
