# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*

COPY object-storage-service/ .

COPY building-block ../building-block

RUN dotnet restore ObjectStorageService.Host/ObjectStorageService.Host.csproj

RUN apt-get update && apt-get install -y git

RUN dotnet publish ObjectStorageService.Host/ObjectStorageService.Host.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:44380
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 44380

ENTRYPOINT ["dotnet", "ObjectStorageService.Host.dll"]