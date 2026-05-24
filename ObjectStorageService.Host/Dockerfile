# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

COPY . .

RUN dotnet restore ObjectStorageService.sln

RUN dotnet publish ObjectStorageService.Host/ObjectStorageService.Host.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ObjectStorageService.Host.dll"]