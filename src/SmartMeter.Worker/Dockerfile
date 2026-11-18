# Build on the host's native arch (fast on a Mac), output for the target arch (arm64 for the Pi).
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY SmartMeter.Worker.csproj .
RUN dotnet restore -a $TARGETARCH
COPY . .
RUN dotnet publish -a $TARGETARCH -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
# Non-root user from the base image; serial access comes from group_add in docker-compose.
USER $APP_UID
ENTRYPOINT ["dotnet", "SmartMeter.Worker.dll"]
