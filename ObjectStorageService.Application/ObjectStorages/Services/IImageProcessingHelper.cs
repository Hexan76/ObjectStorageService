using SixLabors.ImageSharp.Formats;

namespace ObjectStorageService.ObjectStorages.Services;

public interface IImageProcessingHelper
{
    Task<(MemoryStream Stream, IImageFormat Format)> ResizeAsync(Stream input, int width, int height, CancellationToken ct);
    Task<(MemoryStream Stream, IImageFormat Format)> ApplyWatermarkAsync(Stream input, CancellationToken ct);
    string GetContentType(IImageFormat format);
}
