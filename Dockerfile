# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*

COPY object-storage-service/ .

COPY building-block ../building-block

RUN dotnet restore src/ObjectStorageService.Api/ObjectStorageService.Api.csproj

ARG ASSEMBLY_VERSION=1.0.0
ARG VERSION=1.0.0
ARG BUILD_TAG=local

RUN ASSEMBLY_VERSION="$(printf '%s' "$ASSEMBLY_VERSION" | sed -E 's/^(stage|prod)-//')" \
    && echo "Stamping assemblies with $ASSEMBLY_VERSION (version $VERSION, build $BUILD_TAG)" \
    && dotnet publish src/ObjectStorageService.Api/ObjectStorageService.Api.csproj \
        -c Release \
        --no-restore \
        -o /out/api \
        /p:UseAppHost=false \
        /p:Version=$ASSEMBLY_VERSION \
        /p:AssemblyVersion=$ASSEMBLY_VERSION \
        /p:FileVersion=$ASSEMBLY_VERSION \
        /p:InformationalVersion=$VERSION+$BUILD_TAG

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Defaults are for a local build. CI overrides them with --build-arg.
ARG SERVICE_NAME=ObjectStorageService
ARG SERVICE_SCOPE=object-storage
ARG RELEASE_CHANNEL=stage
ARG VERSION=1.0.0
ARG REVISION=local
ARG SOURCE=https://gitlab.bsla.dev/microservice/dotnet/object-storage-service
ARG MIGRATIONS=false
ARG MIGRATION_ENTRYPOINT=none
ARG BUILDING_BLOCK_REVISION=none
ARG EXPOSE_PORT=44380

LABEL com.qased.platform.contract="1" \
    com.qased.platform.service="$SERVICE_NAME" \
    com.qased.platform.scope="$SERVICE_SCOPE" \
    com.qased.platform.release-channel="$RELEASE_CHANNEL" \
    com.qased.platform.migrations="$MIGRATIONS" \
    com.qased.platform.migration-entrypoint="$MIGRATION_ENTRYPOINT" \
    com.qased.build.building-block.revision="$BUILDING_BLOCK_REVISION" \
    org.opencontainers.image.source="$SOURCE" \
    org.opencontainers.image.version="$VERSION" \
    org.opencontainers.image.revision="$REVISION"

COPY --from=build /out/api/ ./

ENV APP_VERSION=${VERSION}
ENV ASPNETCORE_URLS=http://+:${EXPOSE_PORT}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE ${EXPOSE_PORT}

ENTRYPOINT ["dotnet", "ObjectStorageService.Api.dll"]
