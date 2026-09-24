using Framework.BuildingBlock.Application.Contracts;
using ObjectStorageService.ObjectStorages.Services;

namespace ObjectStorageService.ObjectStorages;

public class ThumbnailImageHandler(
    IImageUrlProcessor images,
    INetImageProcessingHelper imageHelper) : IThumbnailImageRequestHandler
{
    public async Task<MessageContract<ProcessImageResult>> Handle(ThumbnailImageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var source = await images.DownloadAsync(request.ImageUrl, request.StorageAppType, cancellationToken);
            await using var stream = source.Stream;
            var thumbnail = await imageHelper.Thumbnail(stream, 300, 300, cancellationToken);
            await using var thumbnailStream = thumbnail.Stream;

            var destination = images.ResolveDestination(request, source.FileName, "thumbnail", thumbnail.Format.Extension);
            await images.UploadAsync(destination.Bucket, destination.Key, thumbnailStream, imageHelper.GetContentType(thumbnail.Format), cancellationToken);
            var url = images.BuildUrl(destination.Bucket, destination.Key);

            return MessageContract<ProcessImageResult>.Success(new ProcessImageResult
            {
                FileName = source.FileName,
                SourceUrl = request.ImageUrl,
                URL = url,
                MimeType = imageHelper.GetContentType(thumbnail.Format),
                Variants = new Dictionary<string, string> { ["thumbnail"] = url }
            });
        }
        catch (Exception ex)
        {
            return MessageContract<ProcessImageResult>.Success(images.Failed(request.ImageUrl, ex.Message));
        }
    }
}
