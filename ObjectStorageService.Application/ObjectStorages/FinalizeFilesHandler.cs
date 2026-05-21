using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Options;
using ObjectStorageService;
using ObjectStorageService.ObjectStorages;
using ObjectStorageService.ObjectStorages.Services;

public class FinalizeFilesHandler(
    IImageProcessingHelper _imageHelper,
    IMinioProcessingHelper _minioHelper,
    IOptions<ObjectStorageOptions> options)
    : IFinalizeFilesRequestHandler
{
    private readonly string _bucket = options.Value.BucketDestination;
    private readonly string _baseUrl = options.Value.PublicBaseUrl;

    public async Task<MessageContract<FinalizeFilesResponse>> Handle(
        FinalizeFilesRequest request,
        CancellationToken cancellationToken)
    {
        // watermark is loaded and cached by `_imageHelper` during construction
        var result = new FinalizeFilesResponse();

        foreach (var file in request.Files)
        {
            var tempKey = file.ObjectKey;

            // ---------------------------
            // METADATA
            // ---------------------------
            var mimeType = await _minioHelper.StatObjectContentTypeAsync(_bucket, tempKey, cancellationToken);
            var extension = MimeToExtension(mimeType);

            var domain = ResolveDomainByExtension(extension);
            var entityType = file.StorageEntityType.ToString().ToLower();

            var basePath = $"{domain}/{entityType}/{file.EntityKey}";
            var variants = new List<FileVariant>();

            // ---------------------------
            // ORIGINAL
            // ---------------------------

            string fileName = file.FileName;
            if (fileName is null)
            {
                var tags = await _minioHelper.GetObjectTagsAsync(_bucket, tempKey, cancellationToken);
                fileName = Path.GetFileNameWithoutExtension(
                    tags.FirstOrDefault().Value);
            }

            var originalKey = $"{basePath}/{fileName}{extension}";

            await _minioHelper.CopyObjectAsync(_bucket, originalKey, tempKey, cancellationToken);
            variants.Add(ToVariant("original", originalKey));

            // ---------------------------
            // THUMBNAIL
            // ---------------------------
            if (file.GenerateThumbnail)
            {

                var thumbKey = $"{basePath}/{fileName}_thumbnail{extension}";

                var src = await _minioHelper.GetObjectAsStreamAsync(_bucket, tempKey, cancellationToken);
                var (outStream, format) = await _imageHelper.ResizeAsync(src, 300, 300, cancellationToken);

                await _minioHelper.PutObjectAsync(_bucket, thumbKey, outStream, _imageHelper.GetContentType(format), cancellationToken);

                variants.Add(ToVariant("thumbnail", thumbKey));
            }

            // ---------------------------
            // SIZES
            // ---------------------------
            foreach (var size in file.Sizes)
            {
                var (w, h) = size switch
                {
                    ImageSize.Small => (300, 300),
                    ImageSize.Medium => (800, 800),
                    ImageSize.Large => (1600, 1600),
                    _ => (800, 800)
                };

                var key = $"{basePath}/{fileName}_{size.ToString().ToLower()}{extension}";

                var src = await _minioHelper.GetObjectAsStreamAsync(_bucket, tempKey, cancellationToken);
                var (outStream, format) = await _imageHelper.ResizeAsync(src, w, h, cancellationToken);

                await _minioHelper.PutObjectAsync(_bucket, key, outStream, _imageHelper.GetContentType(format), cancellationToken);

                variants.Add(ToVariant(size.ToString().ToLower(), key));
            }

            // ---------------------------
            // WATERMARK
            // ---------------------------
            if (file.Watermark)
            {
                var wmKey = $"{basePath}/{fileName}_watermark{extension}";

                var src = await _minioHelper.GetObjectAsStreamAsync(_bucket, tempKey, cancellationToken);
                var (outStream, format) = await _imageHelper.ApplyWatermarkAsync(src, cancellationToken);

                await _minioHelper.PutObjectAsync(_bucket, wmKey, outStream, _imageHelper.GetContentType(format), cancellationToken);

                variants.Add(ToVariant("watermark", wmKey));
            }

            // ---------------------------
            // DELETE TEMP
            // ---------------------------
            await _minioHelper.RemoveObjectAsync(_bucket, tempKey, cancellationToken);

            // ---------------------------
            // RESPONSE
            // ---------------------------
            result.Files.Add(new FinalizeItemResponse
            {
                ObjectKey = file.ObjectKey,
                URL = BuildUrl(originalKey),
                Variants = variants
            });
        }

        return new AcceptMessage<FinalizeFilesResponse>
        {
            Data = result
        };
    }

    // =====================================================
    // HELPERS
    // =====================================================
    private FileVariant ToVariant(string type, string key)
        => new() { Type = type, Url = BuildUrl(key) };

    private string BuildUrl(string key)
        => $"{_baseUrl.TrimEnd('/')}/{_bucket}/{key}";

    private static string ResolveDomainByExtension(string extension)
        => extension.ToLower() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" => "media",
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" => "docs",
            _ => "obj"
        };

    private static string MimeToExtension(string mime)
        => mime switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "application/pdf" => ".pdf",
            _ => ".bin"
        };


}