# SmartMeter worker

.NET 10 background service that reads the DSMR P1 port, validates the CRC, parses the telegram,
and POSTs it to the meter-data API every 5 minutes.

## Deploy on the Raspberry Pi
```bash
uname -m                        # aarch64 expected (64-bit Pi OS)
ls -l /dev/serial/by-id/        # find the P1 cable; put that path in docker-compose.yml `devices:`
getent group dialout            # gid for `group_add` (usually 20)

docker compose pull             # latest image from ghcr.io
docker compose up -d
docker compose logs -f          # expect: "Meter ...: usage x kW ... -> HTTP 200"
```
After a few good cycles, remove the old cron entry (`crontab -e`).

Update later with `docker compose pull && docker compose up -d`. To build on the Pi itself instead of
pulling, use `docker compose up -d --build`.

Building on the Mac instead: `docker buildx build --platform linux/arm64 -t smartmeter-worker src/SmartMeter.Worker`.

## Published image
`.github/workflows/docker-publish.yml` runs the tests and builds `ghcr.io/bgflor25/smartmeterapiparser`
for `linux/amd64` and `linux/arm64`:
- push to `main` → tags `latest` and `sha-<commit>`
- tag `vX.Y.Z` → tags `X.Y.Z` and `X.Y`
- pull requests → build only, nothing published

If the package is private, log in on the Pi first with a token that has `read:packages`:
`echo <token> | docker login ghcr.io -u BGflor25 --password-stdin`.

## Configuration
`appsettings.json` → `Meter` section, overridable with env vars (`Meter__ApiUrl`, `Meter__Interval`,
`Meter__PortName`, `Meter__ReadTimeout`, `Meter__BaudRate`, `Meter__DtrEnable`, `Meter__RtsEnable`).

`DtrEnable`/`RtsEnable` default to `true`: many P1 cables power the meter's data-request line from
DTR/RTS, and the meter sends nothing while they are low (.NET lowers them by default, `cat` and pyserial don't).

Local test without a meter: set `Meter__SourceFile=/path/to/output.txt` and the last valid telegram in that
file is used instead of the serial port.

## Notes
- If the USB cable is unplugged while running, restart the container (devices are bound at start).
- Tariff mapping is unchanged from the Python script: `1.8.1` → `*_low_tariff`. On Belgian meters
  tariff 1 is actually day/peak, so check this against what the API expects.

## Tests
`dotnet test`. Fixtures in `tests/SmartMeter.Tests/Fixtures`, including `python-output.json` produced by the
original Python script, which the C# payload is compared against.
