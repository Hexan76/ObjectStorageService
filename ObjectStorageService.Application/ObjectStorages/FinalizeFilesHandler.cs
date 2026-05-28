using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ObjectStorageService;
using ObjectStorageService.Domain;
using ObjectStorageService.ObjectStorages;
using ObjectStorageService.ObjectStorages.Services;
using Volo.Abp.Caching;

public class FinalizeFilesHandler(
    IImageProcessingHelper _imageHelper,
    IMinioProcessingHelper _minioHelper,
    IDistributedCache<ObjectTempCache> distributedCache,
    IOptions<ObjectStorageOptions> options) : IFinalizeFilesRequestHandler
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
            var tempObject = await distributedCache.GetAsync(file.Id.ToString());
            var tempKey = tempObject.ObjectKey;

            // ---------------------------
            // METADATA
            // ---------------------------
            var mimeType = await _minioHelper.StatObjectContentTypeAsync(_bucket, tempKey, cancellationToken);
            var extension = MimeToExtension(mimeType);


            var domain = ResolveDomainByExtension(extension);
            var entityType = file.StorageEntityType.ToString().ToLower();

            var basePath = $"{domain}/{entityType}/{file.EntityKey}";
            var variants = new List<FileVariant>();

            var src = await _minioHelper.GetObjectAsStreamAsync(_bucket, tempKey, cancellationToken);

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

            var originalKey = GetStorageKeyFileName(basePath, fileName, extension);

            await _minioHelper.CopyObjectAsync(_bucket, originalKey, tempKey, cancellationToken);
            variants.Add(ToVariant("original", originalKey));


            if ("media" == domain)
            {
                // ---------------------------
                // Compressed
                // ---------------------------
                var compressedKey = GetStorageKeyFileName(basePath, fileName, "webp");

                var compressedLoaded = await _imageHelper.CompressToWebp(src, 50 * 1024, cancellationToken);

                await _minioHelper.PutObjectAsync(_bucket, compressedKey, compressedLoaded.Stream, _imageHelper.GetContentType(compressedLoaded.Format), cancellationToken);

                variants.Add(ToVariant("Compressed", compressedKey));

                // ---------------------------
                // THUMBNAIL
                // ---------------------------
                if (file.GenerateThumbnail)
                {
                    var (outStream, format) = await _imageHelper.Thumbnail(src, 300, 300, cancellationToken);

                    var thumbKey = GetStorageKeyFileName(basePath, $"{fileName}_thumbnail", format.FileExtensions.First());

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


                    var (outStream, format) = await _imageHelper.ResizeAsync(src, w, h, cancellationToken);
                    var key = GetStorageKeyFileName(basePath, $"{fileName}_{size.ToString().ToLower()}", format.FileExtensions.First());

                    await _minioHelper.PutObjectAsync(_bucket, key, outStream, _imageHelper.GetContentType(format), cancellationToken);

                    variants.Add(ToVariant(size.ToString().ToLower(), key));
                }

                // ---------------------------
                // WATERMARK
                // ---------------------------
                if (file.Watermark)
                {
                    var (outStream, format) = await _imageHelper.ApplyWatermarkAsync(src, cancellationToken);

                    var wmKey = GetStorageKeyFileName(basePath, $"{fileName}_watermark", format.FileExtensions.First());

                    await _minioHelper.PutObjectAsync(_bucket, wmKey, outStream, _imageHelper.GetContentType(format), cancellationToken);

                    variants.Add(ToVariant("watermark", wmKey));
                }

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
                Id = file.Id,
                URL = BuildUrl(originalKey),
                Variants = variants
            });

            await distributedCache.RemoveAsync(file.Id.ToString());
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
            "jpg" or "jpeg" or "png" or "webp" or "gif" or "bmp" => "media",
            "pdf" or "doc" or "docx" or "xls" or "xlsx" => "docs",
            _ => "obj"
        };

    private static string MimeToExtension(string mime)
        => mime?.ToLowerInvariant() switch
        {
            // images
            "image/jpeg" or "image/jpg" or "image/pjpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            "image/gif" => "gif",
            "image/bmp" or "image/x-ms-bmp" => "bmp",
            "image/tiff" or "image/tif" => "tiff",
            "image/svg+xml" => "svg",
            "image/avif" => "avif",
            "image/heic" or "image/heif" => "heic",

            // documents
            "application/pdf" => "pdf",
            "application/msword" => "doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
            "application/vnd.ms-excel" => "xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
            "application/vnd.ms-powerpoint" => "ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => "pptx",

            // text
            "text/plain" => "txt",
            "text/csv" => "csv",
            "text/html" => "html",
            "text/css" => "css",
            "text/javascript" => "js",

            // archives
            "application/zip" => "zip",
            "application/x-7z-compressed" => "7z",
            "application/x-rar-compressed" => "rar",
            "application/gzip" => "gz",
            "application/x-tar" => "tar",

            // audio
            "audio/mpeg" => "mp3",
            "audio/wav" => "wav",
            "audio/ogg" => "ogg",
            "audio/webm" => "webm",

            // video
            "video/mp4" => "mp4",
            "video/x-msvideo" => "avi",
            "video/x-matroska" => "mkv",
            "video/webm" => "webm",

            // fallback
            _ => "bin"
        };

    private static string GetStorageKeyFileName(string basePath, string fileName, string extension)
    {
        return $"{basePath}/{fileName}.{extension}";
    }

}