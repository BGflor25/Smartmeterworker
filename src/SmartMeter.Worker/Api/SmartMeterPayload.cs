using System.Text.Json.Serialization;

namespace SmartMeter.Worker.Api;

public sealed record ValueUnit(
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("unit")] string Unit);

/// <summary>
/// Mirrors the SmartMeterDataDto the API expects. Keys and types match the JSON the
/// former parse_dsmr.py produced (including the raw OBIS-code keys).
/// </summary>
public sealed class SmartMeterPayload
{
    [JsonPropertyName("meter_model")] public double MeterModel { get; init; }
    [JsonPropertyName("equipment_identifier")] public double EquipmentIdentifier { get; init; }
    [JsonPropertyName("timestamp")] public double Timestamp { get; init; }

    [JsonPropertyName("electricity_import_low_tariff")] public required ValueUnit ElectricityImportLowTariff { get; init; }
    [JsonPropertyName("electricity_import_high_tariff")] public required ValueUnit ElectricityImportHighTariff { get; init; }
    [JsonPropertyName("electricity_export_low_tariff")] public required ValueUnit ElectricityExportLowTariff { get; init; }
    [JsonPropertyName("electricity_export_high_tariff")] public required ValueUnit ElectricityExportHighTariff { get; init; }
    [JsonPropertyName("tariff_indicator")] public double TariffIndicator { get; init; }

    [JsonPropertyName("1-0:1.4.0")] public required ValueUnit CurrentAverageDemand { get; init; }
    [JsonPropertyName("1-0:1.6.0")] public double MaxDemandThisMonth { get; init; }
    [JsonPropertyName("0-0:98.1.0")] public double MaxDemandHistory { get; init; }

    [JsonPropertyName("current_power_usage")] public required ValueUnit CurrentPowerUsage { get; init; }
    [JsonPropertyName("current_power_delivery")] public required ValueUnit CurrentPowerDelivery { get; init; }
    [JsonPropertyName("instantaneous_active_power_l1_pos")] public required ValueUnit InstantaneousActivePowerL1Pos { get; init; }
    [JsonPropertyName("instantaneous_active_power_l2_pos")] public required ValueUnit InstantaneousActivePowerL2Pos { get; init; }

    [JsonPropertyName("voltage")] public required ValueUnit Voltage { get; init; }
    [JsonPropertyName("current")] public required ValueUnit Current { get; init; }

    [JsonPropertyName("0-0:96.3.10")] public double BreakerState { get; init; }
    [JsonPropertyName("0-0:17.0.0")] public required ValueUnit LimiterThreshold { get; init; }
    [JsonPropertyName("max_current")] public required ValueUnit MaxCurrent { get; init; }

    [JsonPropertyName("0-1:24.1.0")] public double Channel1DeviceType { get; init; }
    [JsonPropertyName("gas_device_id")] public double GasDeviceId { get; init; }
    [JsonPropertyName("0-1:24.4.0")] public double GasValveState { get; init; }
    [JsonPropertyName("gas_consumption")] public double GasConsumption { get; init; }

    [JsonPropertyName("water_consumption")] public double WaterConsumption { get; init; }

    [JsonPropertyName("checksum")] public string Checksum { get; init; } = "";
}
