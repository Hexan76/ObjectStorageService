using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Template.Service.Domain.Shared;
using Volo.Abp.DependencyInjection;

namespace ObjectStorageService.ObjectStorages.Services;

public interface IImageUrlProcessor
{
    Task<DownloadedImage> DownloadAsync(string imageUrl, StorageAppType appType, CancellationToken ct);
    string GetBucket(StorageAppType appType);
    string BuildUrl(string bucket, string key);
    string BuildObjectKey(string entityType, string entityKey, string fileName, string suffix, string extension);
    (string Bucket, string Key) ResolveDestination(ImageUrlProcessRequest request, string fileName, string suffix, string extension);
    Task UploadAsync(string bucket, string key, Stream data, string contentType, CancellationToken ct);
    ProcessImageResult Failed(string? sourceUrl, string? error);
    string Localize(string? key, object[]? args = null);
}

public sealed record DownloadedImage(MemoryStream Stream, string FileName, string ContentType, string Extension);

[ExposeServices(typeof(IImageUrlProcessor))]
public sealed class ImageUrlProcessor(
    IMinioProcessingHelper minioHelper,
    IOptions<ObjectStorageOptions> options,
    IStringLocalizer<ObjectStorageServiceResource> L) : IImageUrlProcessor, IScopedDependency
{
    private readonly string _baseUrl = options.Value.PublicBaseUrl;
    private readonly string _environment = options.Value.Environment;

    public async Task<DownloadedImage> DownloadAsync(string imageUrl, StorageAppType appType, CancellationToken ct)
    {
        var (bucket, objectKey) = ParseSource(imageUrl, appType);
        var stat = await minioHelper.StatObjectAsync(bucket, objectKey, ct);
        var stream = await minioHelper.GetObjectAsStreamAsync(bucket, objectKey, ct);
        var contentType = string.IsNullOrWhiteSpace(stat.ContentType) ? "application/octet-stream" : stat.ContentType;
        var extension = ExtensionFrom(contentType, objectKey);

        if (!IsRasterImage(extension))
            throw new InvalidOperationException(L[FileLocalizationKeys.NotAnImage, objectKey]);

        if (stream.Length == 0)
            throw new InvalidOperationException(L[FileLocalizationKeys.Empty]);

        return new DownloadedImage(stream, SanitizeFileName(Path.GetFileNameWithoutExtension(objectKey)), contentType, extension);
    }

    public string GetBucket(StorageAppType appType)
        => StorageAppMapping.GetStorageAppType(appType, _environment);

    public string BuildUrl(string bucket, string key)
        => $"{_baseUrl.TrimEnd('/')}/{bucket}/{key}";

    public string BuildObjectKey(string entityType, string entityKey, string fileName, string suffix, string extension)
    {
        var name = string.IsNullOrWhiteSpace(suffix) ? fileName : $"{fileName}_{suffix}";
        return $"media/{entityType.ToLowerInvariant()}/{entityKey}/{name}.{extension.TrimStart('.')}";
    }

    public (string Bucket, string Key) ResolveDestination(ImageUrlProcessRequest request, string fileName, string suffix, string extension)
    {
        if (!request.IsCurrentDirectory)
            return (GetBucket(request.StorageAppType), BuildObjectKey(request.StorageEntityType, request.EntityKey, fileName, suffix, extension));

        var (bucket, objectKey) = ParseSource(request.ImageUrl, request.StorageAppType);
        var sourceName = SanitizeFileName(Path.GetFileNameWithoutExtension(objectKey));
        var directory = Path.GetDirectoryName(objectKey)?.Replace('\\', '/');
        var name = string.IsNullOrWhiteSpace(suffix) ? sourceName : $"{sourceName}_{suffix}";
        var key = string.IsNullOrEmpty(directory)
            ? $"{name}.{extension.TrimStart('.')}"
            : $"{directory}/{name}.{extension.TrimStart('.')}";

        return (bucket, key);
    }

    public Task UploadAsync(string bucket, string key, Stream data, string contentType, CancellationToken ct)
        => minioHelper.PutObjectAsync(bucket, key, data, contentType, null, ct);

    public ProcessImageResult Failed(string? sourceUrl, string? error)
        => new()
        {
            SourceUrl = sourceUrl ?? string.Empty,
            Success = false,
            Error = error
        };

    public string Localize(string? key, object[]? args = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return L[FileLocalizationKeys.ProcessFailed];

        return args is { Length: > 0 }
            ? L[key, args]
            : L[key];
    }

    private (string Bucket, string ObjectKey) ParseSource(string imageUrl, StorageAppType appType)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(L[FileLocalizationKeys.InvalidImageUrl]);
        }

        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            throw new InvalidOperationException(L[FileLocalizationKeys.InvalidImageUrl]);

        if (segments[0] is "media" or "docs" or "obj")
            return (GetBucket(appType), string.Join('/', segments));

        return (segments[0], string.Join('/', segments[1..]));
    }

    private static string SanitizeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "image";

        name = Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "_");
        return name[..Math.Min(name.Length, 80)];
    }

    private static string ExtensionFrom(string contentType, string objectKey)
    {
        var fromMime = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" or "image/pjpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            "image/gif" => "gif",
            "image/bmp" or "image/x-ms-bmp" => "bmp",
            "image/tiff" or "image/tif" => "tiff",
            "image/avif" => "avif",
            "image/heic" or "image/heif" => "heic",
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(fromMime))
            return fromMime;

        return Path.GetExtension(objectKey).TrimStart('.').ToLowerInvariant();
    }

    private static bool IsRasterImage(string extension)
        => extension is
            "jpg" or "jpeg" or "png" or "webp" or "gif" or
            "bmp" or "tiff" or "tif" or "avif" or "heic" or "heif";
}
