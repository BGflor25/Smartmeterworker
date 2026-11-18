using SmartMeter.Worker.Parsing;

namespace SmartMeter.Worker.Api;

public static class PayloadBuilder
{
    public static SmartMeterPayload Build(Telegram t)
    {
        double N(string obis) => t.GetNumber(obis) ?? 0;
        ValueUnit Vu(string obis, string unit) => new(N(obis), unit);

        return new SmartMeterPayload
        {
            MeterModel = N("0-0:96.1.4"),
            // The API types these as numbers, but real serials ("1SAG...") contain letters.
            EquipmentIdentifier = 0,
            Timestamp = DsmrTimestamp.ToUnixSeconds(t.GetRaw("0-0:1.0.0")) ?? 0,

            // Mapping kept identical to parse_dsmr.py (tariff 1 -> "low").
            // Note: on Belgian meters tariff 1 is day/peak and tariff 2 is night.
            ElectricityImportLowTariff = Vu("1-0:1.8.1", "kWh"),
            ElectricityImportHighTariff = Vu("1-0:1.8.2", "kWh"),
            ElectricityExportLowTariff = Vu("1-0:2.8.1", "kWh"),
            ElectricityExportHighTariff = Vu("1-0:2.8.2", "kWh"),
            TariffIndicator = N("0-0:96.14.0"),

            CurrentAverageDemand = Vu("1-0:1.4.0", "kW"),
            MaxDemandThisMonth = N("1-0:1.6.0"),
            // A list of up to 13 monthly peaks; doesn't fit a single number.
            MaxDemandHistory = 0,

            CurrentPowerUsage = Vu("1-0:1.7.0", "kW"),
            CurrentPowerDelivery = Vu("1-0:2.7.0", "kW"),
            InstantaneousActivePowerL1Pos = Vu("1-0:21.7.0", "kW"),
            InstantaneousActivePowerL2Pos = Vu("1-0:41.7.0", "kW"),

            Voltage = Vu("1-0:32.7.0", "V"),
            Current = Vu("1-0:31.7.0", "A"),

            BreakerState = N("0-0:96.3.10"),
            LimiterThreshold = Vu("0-0:17.0.0", "kW"),
            MaxCurrent = Vu("1-0:31.4.0", "A"),

            Channel1DeviceType = N("0-1:24.1.0"),
            GasDeviceId = 0,
            GasValveState = N("0-1:24.4.0"),
            GasConsumption = N("0-1:24.2.3"),

            WaterConsumption = N("0-2:24.2.1"),

            Checksum = t.Checksum ?? "",
        };
    }
}
