using Framework.BuildingBlock.Application.Contracts;
using ObjectStorageService.ObjectStorages.Services;

namespace ObjectStorageService.ObjectStorages;

public class WatermarkImageHandler(
    IImageUrlProcessor images,
    INetImageProcessingHelper imageHelper) : IWatermarkImageRequestHandler
{
    public async Task<MessageContract<ProcessImageResult>> Handle(WatermarkImageRequest request, CancellationToken cancellationToken)
    {
        if (request.WatermarkSvg is null || request.WatermarkSvg.Length == 0 ||
            !Path.GetExtension(request.WatermarkSvg.FileName).Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return MessageContract<ProcessImageResult>.Success(
                images.Failed(request.ImageUrl, images.Localize(FileLocalizationKeys.WatermarkRequired)));
        }

        try
        {
            var source = await images.DownloadAsync(request.ImageUrl, request.StorageAppType, cancellationToken);
            await using var stream = source.Stream;
            await using var watermark = request.WatermarkSvg.OpenReadStream();
            var watermarked = await imageHelper.ApplyWatermarkAsync(stream, watermark, cancellationToken);
            await using var watermarkedStream = watermarked.Stream;

            var destination = images.ResolveDestination(request, source.FileName, "watermark", watermarked.Format.Extension);
            await images.UploadAsync(destination.Bucket, destination.Key, watermarkedStream, imageHelper.GetContentType(watermarked.Format), cancellationToken);
            var url = images.BuildUrl(destination.Bucket, destination.Key);

            return MessageContract<ProcessImageResult>.Success(new ProcessImageResult
            {
                FileName = source.FileName,
                SourceUrl = request.ImageUrl,
                URL = url,
                MimeType = imageHelper.GetContentType(watermarked.Format),
                Variants = new Dictionary<string, string> { ["watermark"] = url }
            });
        }
        catch (Exception ex)
        {
            return MessageContract<ProcessImageResult>.Success(images.Failed(request.ImageUrl, ex.Message));
        }
    }
}
