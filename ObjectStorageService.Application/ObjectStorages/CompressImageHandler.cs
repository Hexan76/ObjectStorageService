using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Options;
using ObjectStorageService.ObjectStorages.Services;

namespace ObjectStorageService.ObjectStorages;

public class CompressImageHandler(
    IImageUrlProcessor images,
    INetImageProcessingHelper imageHelper,
    IOptions<ObjectStorageOptions> options) : ICompressImageRequestHandler
{
    public async Task<MessageContract<ProcessImageResult>> Handle(CompressImageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var source = await images.DownloadAsync(request.ImageUrl, request.StorageAppType, cancellationToken);
            await using var stream = source.Stream;
            var compressed = await imageHelper.CompressToWebp(stream, options.Value.DefaultCompressionTargetSize, cancellationToken);
            await using var compressedStream = compressed.Stream;

            var destination = images.ResolveDestination(request, source.FileName, "compressed", compressed.Format.Extension);
            await images.UploadAsync(destination.Bucket, destination.Key, compressedStream, imageHelper.GetContentType(compressed.Format), cancellationToken);
            var url = images.BuildUrl(destination.Bucket, destination.Key);

            return MessageContract<ProcessImageResult>.Success(new ProcessImageResult
            {
                FileName = source.FileName,
                SourceUrl = request.ImageUrl,
                URL = url,
                MimeType = imageHelper.GetContentType(compressed.Format),
                Variants = new Dictionary<string, string> { ["compressed"] = url }
            });
        }
        catch (Exception ex)
        {
            return MessageContract<ProcessImageResult>.Success(images.Failed(request.ImageUrl, ex.Message));
        }
    }
}
