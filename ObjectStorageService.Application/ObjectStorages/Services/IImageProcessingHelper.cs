using SixLabors.ImageSharp.Formats;

namespace ObjectStorageService.ObjectStorages.Services;

public interface IImageProcessingHelper
{
    Task<(MemoryStream Stream, IImageFormat Format)> ResizeAsync(Stream input, int width, int height, CancellationToken ct);
    Task<(MemoryStream Stream, IImageFormat Format)> Thumbnail(Stream input, int width, int height, CancellationToken ct);

    Task<(MemoryStream Stream, IImageFormat Format)> LoadOrginal(Stream input, CancellationToken ct);
    Task<(MemoryStream Stream, IImageFormat Format)> ApplyWatermarkAsync(Stream input, CancellationToken ct);
    Task<(MemoryStream Stream, IImageFormat Format)> CompressToWebp(Stream input, int targetSize, CancellationToken ct);
    string GetContentType(IImageFormat format);
}
public interface INetImageProcessingHelper
{
    Task<(MemoryStream Stream, ImageFormat Format)> ResizeAsync(Stream input, int width, int height, CancellationToken ct = default);
    Task<(MemoryStream Stream, ImageFormat Format)> Thumbnail(Stream input, int width, int height, CancellationToken ct = default);
    Task<(MemoryStream Stream, ImageFormat Format)> ApplyWatermarkAsync(Stream input, CancellationToken ct = default);
    Task<(MemoryStream Stream, ImageFormat Format)> CompressToWebp(Stream input, int targetSize = 50 * 1024, CancellationToken ct = default);
    Task<(MemoryStream Stream, ImageFormat Format)> LoadOrginal(Stream input, CancellationToken ct = default);
    string GetContentType(ImageFormat format);
}
