# SmartMeter worker

.NET 10 background service that reads the DSMR P1 port, validates the CRC, parses the telegram,
and POSTs it to the meter-data API every 5 minutes. It replaces `run_dsmr.sh` + `parse_dsmr.py` + cron.

## Deploy on the Raspberry Pi
```bash
uname -m                        # aarch64 expected (64-bit Pi OS)
ls -l /dev/serial/by-id/        # find the P1 cable; put that path in docker-compose.yml `devices:`
getent group dialout            # gid for `group_add` (usually 20)

docker compose up -d --build
docker compose logs -f          # expect: "Meter ...: usage x kW ... -> HTTP 200"
```
After a few good cycles, remove the old cron entry (`crontab -e`).

Building on the Mac instead: `docker buildx build --platform linux/arm64 -t smartmeter-worker src/SmartMeter.Worker`.

## Configuration
`appsettings.json` → `Meter` section, overridable with env vars (`Meter__ApiUrl`, `Meter__Interval`,
`Meter__PortName`, `Meter__ReadTimeout`, `Meter__BaudRate`).

Local test without a meter: set `Meter__SourceFile=/path/to/output.txt` and the last valid telegram in that
file is used instead of the serial port.

## Notes
- If the USB cable is unplugged while running, restart the container (devices are bound at start).
- Tariff mapping is unchanged from the Python script: `1.8.1` → `*_low_tariff`. On Belgian meters
  tariff 1 is actually day/peak, so check this against what the API expects.

## Tests
`dotnet test`. Fixtures in `tests/SmartMeter.Tests/Fixtures`, including `python-output.json` produced by the
original Python script, which the C# payload is compared against.
