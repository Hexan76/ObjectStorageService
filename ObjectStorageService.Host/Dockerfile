# syntax=docker/dockerfile:1

# =========================
# Build Stage
# =========================
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# Runtime Stage
# =========================
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ObjectStorageService.Host.dll"]