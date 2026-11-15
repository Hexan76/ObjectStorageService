using System.Text.RegularExpressions;
using Framework.BuildingBlock.Application.Contracts;
using ObjectStorageService.ObjectStorages.Services;

namespace ObjectStorageService.ObjectStorages;

public class ResizeImageHandler(
    IImageUrlProcessor images,
    INetImageProcessingHelper imageHelper) : IResizeImageRequestHandler
{
    public async Task<MessageContract<ProcessImageResult>> Handle(ResizeImageRequest request, CancellationToken cancellationToken)
    {
        if (request.Sizes.Count == 0 || request.Sizes.Any(size => size.Width <= 0 || size.Height <= 0))
            return Ok(images.Failed(request.ImageUrl, images.Localize(FileLocalizationKeys.InvalidSizes)));

        try
        {
            var source = await images.DownloadAsync(request.ImageUrl, request.StorageAppType, cancellationToken);
            await using var stream = source.Stream;
            var variants = new Dictionary<string, string>();
            string? firstUrl = null;
            string mimeType = source.ContentType;

            for (var i = 0; i < request.Sizes.Count; i++)
            {
                var size = request.Sizes[i];
                stream.Position = 0;
                var resized = await imageHelper.ResizeAsync(stream, size.Width, size.Height, cancellationToken);
                await using var resizedStream = resized.Stream;
                var suffix = SizeName(size, i);
                mimeType = imageHelper.GetContentType(resized.Format);
                var destination = images.ResolveDestination(request, source.FileName, suffix, resized.Format.Extension);
                await images.UploadAsync(destination.Bucket, destination.Key, resizedStream, imageHelper.GetContentType(resized.Format), cancellationToken);
                var url = images.BuildUrl(destination.Bucket, destination.Key);
                variants[suffix] = url;
                firstUrl ??= url;
            }

            return Ok(new ProcessImageResult
            {
                FileName = source.FileName,
                SourceUrl = request.ImageUrl,
                URL = firstUrl ?? string.Empty,
                MimeType = mimeType,
                Success = true,
                Variants = variants
            });
        }
        catch (Exception ex)
        {
            return Ok(images.Failed(request.ImageUrl, ex.Message));
        }
    }

    private static MessageContract<ProcessImageResult> Ok(ProcessImageResult result)
        => MessageContract<ProcessImageResult>.Success(result);

    private static string SizeName(ProcessImageSize size, int index)
    {
        var name = string.IsNullOrWhiteSpace(size.Name)
            ? $"{size.Width}x{size.Height}"
            : size.Name.Trim().ToLowerInvariant();

        name = Regex.Replace(name, @"[^a-z0-9_\-]", "_");
        return string.IsNullOrWhiteSpace(name) ? $"size{index}" : name;
    }
}
