using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Options;
using ObjectStorageService;
using ObjectStorageService.ObjectStorages;
using ObjectStorageService.ObjectStorages.Services;

public class PromoteFilesHandler(
    IImageProcessingHelper _imageHelper,
    IMinioProcessingHelper _minioHelper,
    IOptions<ObjectStorageOptions> options) : IPromoteFilesRequestHandler
{
    private string _bucket = options.Value.BucketDestination;
    private readonly string _baseUrl = options.Value.PublicBaseUrl;
    private readonly string _environment = options.Value.Environment;

    public async Task<MessageContract<PromoteFilesResponse>> Handle(
        PromoteFilesRequest request,
        CancellationToken cancellationToken)
    {
        // watermark is loaded and cached by `_imageHelper` during construction
        var result = new PromoteFilesResponse();

        var uploadTasks = new List<Task>();
        foreach (var file in request.Files)
        {
            _bucket = StorageAppMapping.GetStorageAppType(file.AppType, _environment);

            // ---------------------------
            // METADATA
            // ---------------------------
            var objectKey = GetObjectKeyFromUrl(file.URL, _bucket);


            var stat = await _minioHelper.StatObjectAsync(_bucket, objectKey, cancellationToken);
            var extension = MimeToExtension(stat.ContentType);


            var domain = ResolveDomainByExtension(extension);
            var entityType = file.StorageEntityType.ToLower();

            var basePath = $"{domain}/{entityType}/{file.EntityKey}";
            var variants = new List<FileVariant>();

            var src = await _minioHelper.GetObjectAsStreamAsync(_bucket, objectKey, cancellationToken);




            // ---------------------------
            // ORIGINAL
            // ---------------------------

            string fileName = file.FileName;
            if (fileName is null)
            {
                var tags = await _minioHelper.GetObjectTagsAsync(_bucket, objectKey, cancellationToken);
                fileName = Path.GetFileNameWithoutExtension(
                    tags.FirstOrDefault().Value);
            }

            var originalKey = GetStorageKeyFileName(basePath, fileName, extension);

            // Original
            uploadTasks.Add(
                _minioHelper.CopyObjectAsync(
                    _bucket,
                    originalKey,
                    objectKey,
                    cancellationToken));

            variants.Add(ToVariant("original", originalKey));


            if ("media" == domain)
            {
                // ---------------------------
                // Compressed
                // ---------------------------
                var compressedKey = GetStorageKeyFileName(basePath, fileName, "webp");

                var compressedLoaded = await _imageHelper.CompressToWebp(src, 50 * 1024, cancellationToken);
                uploadTasks.Add(
                        _minioHelper.PutObjectAsync(
                            _bucket,
                            compressedKey,
                            compressedLoaded.Stream,
                            _imageHelper.GetContentType(compressedLoaded.Format),
                            cancellationToken));


                variants.Add(ToVariant("compressed", compressedKey));

                // ---------------------------
                // THUMBNAIL
                // ---------------------------
                if (file.GenerateThumbnail)
                {
                    var (outStream, format) = await _imageHelper.Thumbnail(src, 300, 300, cancellationToken);

                    var thumbKey = GetStorageKeyFileName(basePath, $"{fileName}_thumbnail", format.FileExtensions.First());

                    uploadTasks.Add(
                            _minioHelper.PutObjectAsync(
                                _bucket,
                                thumbKey,
                                outStream,
                                _imageHelper.GetContentType(format),
                                cancellationToken));


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

                    uploadTasks.Add(
                        _minioHelper.PutObjectAsync(
                            _bucket,
                            key,
                            outStream,
                            _imageHelper.GetContentType(format),
                            cancellationToken));


                    variants.Add(ToVariant(size.ToString().ToLower(), key));
                }

                // ---------------------------
                // WATERMARK
                // ---------------------------
                if (file.Watermark)
                {
                    var (outStream, format) = await _imageHelper.ApplyWatermarkAsync(src, cancellationToken);

                    var wmKey = GetStorageKeyFileName(basePath, $"{fileName}_watermark", format.FileExtensions.First());


                    uploadTasks.Add(
                        _minioHelper.PutObjectAsync(
                            _bucket,
                            wmKey,
                            outStream,
                            _imageHelper.GetContentType(format),
                            cancellationToken));

                    variants.Add(ToVariant("watermark", wmKey));
                }

            }

            // ---------------------------
            // DELETE TEMP
            // ---------------------------
            if (request.IsTransaction is true)
                await _minioHelper.RemoveObjectAsync(_bucket, objectKey, cancellationToken);

            // ---------------------------
            // RESPONSE
            // ---------------------------


            result.Files.Add(new PromoteItemResponse
            {
                FileName = stat.ObjectName,
                MimeType = stat.ContentType,
                StorageKey = objectKey,
                Size = stat.Size,
                URL = BuildUrl(originalKey),
                Variants = variants
                    .GroupBy(x => x.Type)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().Url
                    )
            });

        }

        await Task.WhenAll(uploadTasks);
        return new AcceptMessage<PromoteFilesResponse>
        {
            Result = result
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
    private static string GetObjectKeyFromUrl(string url, string bucket)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }

        var uri = new Uri(url);

        var path = uri.AbsolutePath.TrimStart('/');

        if (path.StartsWith(bucket + "/", StringComparison.OrdinalIgnoreCase))
            path = path[(bucket.Length + 1)..];

        return Uri.UnescapeDataString(path);
    }

}