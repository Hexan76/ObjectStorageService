using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using ObjectStorageService;
using ObjectStorageService.ObjectStorages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Svg.Skia;

public class FinalizeFilesHandler(
    IOptions<ObjectStorageOptions> options)
    : IFinalizeFilesRequestHandler
{
    private readonly string _bucket = options.Value.BucketDestination;
    private readonly string _baseUrl = options.Value.PublicBaseUrl;

    private readonly IMinioClient _client = new MinioClientFactory(cfg =>
    {
        cfg.WithEndpoint(options.Value.URLDestination)
           .WithCredentials(options.Value.DestinationKey, options.Value.DestinationSecret)
           .WithSSL(options.Value.DestinationSSL)
           .Build();
    }).CreateClient();

    static Image<Rgba32>? CachedWatermark;

    public async Task<MessageContract<FinalizeFilesResponse>> Handle(
        FinalizeFilesRequest request,
        CancellationToken cancellationToken)
    {
        CachedWatermark = LoadSvgAsImage(options.Value.SvgFileFullName);

        var result = new FinalizeFilesResponse();

        foreach (var file in request.Files)
        {
            var tempKey = file.ObjectKey;

            // ---------------------------
            // METADATA
            // ---------------------------
            var stat = await _client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(tempKey),
                cancellationToken);

            var mimeType = stat.ContentType;
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
                var tags = await _client.GetObjectTagsAsync(
                        new GetObjectTagsArgs()
                            .WithBucket(_bucket)
                            .WithObject(tempKey),
                        cancellationToken);
                fileName = tags.Tags.FirstOrDefault().Value;
            }

            var originalKey = $"{basePath}/{fileName}{extension}";

            await CopyObject(tempKey, originalKey, cancellationToken);
            variants.Add(ToVariant("original", originalKey));

            // ---------------------------
            // THUMBNAIL
            // ---------------------------
            if (file.GenerateThumbnail)
            {

                var thumbKey = $"{basePath}/{fileName}_thumbnail{extension}";

                await GenerateImageVariant(
                    tempKey,
                    thumbKey,
                    300,
                    300,
                    cancellationToken);

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

                await GenerateImageVariant(
                    tempKey,
                    key,
                    w,
                    h,
                    cancellationToken);

                variants.Add(ToVariant(size.ToString().ToLower(), key));
            }

            // ---------------------------
            // WATERMARK
            // ---------------------------
            if (file.Watermark)
            {
                var wmKey = $"{basePath}/{fileName}_watermark{extension}";

                await GenerateWatermark(
                    tempKey,
                    wmKey,
                    cancellationToken);

                variants.Add(ToVariant("watermark", wmKey));
            }

            // ---------------------------
            // DELETE TEMP
            // ---------------------------
            await _client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(tempKey),
                cancellationToken);

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
    // IMAGE VARIANT
    // =====================================================
    private async Task GenerateImageVariant(
        string sourceKey,
        string targetKey,
        int width,
        int height,
        CancellationToken ct)
    {
        using var ms = new MemoryStream();

        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(sourceKey)
                .WithCallbackStream(s => s.CopyToAsync(ms, ct)),
            ct);

        ms.Position = 0;

        using var image = await Image.LoadAsync(ms, ct);
        var format = image.Metadata.DecodedImageFormat;

        image.Mutate(x =>
        {
            x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(width, height)
            });
        });

        using var outStream = new MemoryStream();

        await SaveWithOriginalFormat(image, outStream, format);

        outStream.Position = 0;

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(targetKey)
                .WithStreamData(outStream)
                .WithObjectSize(outStream.Length)
                .WithContentType(GetContentType(format)),
            ct);
    }

    // =====================================================
    // WATERMARK
    // =====================================================
    private async Task GenerateWatermark(
        string sourceKey,
        string targetKey,
        CancellationToken ct)
    {
        using var ms = new MemoryStream();

        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(sourceKey)
                .WithCallbackStream(s => s.CopyToAsync(ms, ct)),
            ct);

        ms.Position = 0;

        using var image = await Image.LoadAsync(ms, ct);
        var format = image.Metadata.DecodedImageFormat;

        image.Mutate(ctx =>
        {
            ApplyWatermarkInternal(ctx, image);
        });

        using var outStream = new MemoryStream();

        await SaveWithOriginalFormat(image, outStream, format);

        outStream.Position = 0;

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(targetKey)
                .WithStreamData(outStream)
                .WithObjectSize(outStream.Length)
                .WithContentType(GetContentType(format)),
            ct);
    }

    // =====================================================
    // COPY OBJECT
    // =====================================================
    private async Task CopyObject(
        string source,
        string dest,
        CancellationToken ct)
    {
        await _client.CopyObjectAsync(
            new CopyObjectArgs()
                .WithBucket(_bucket)
                .WithObject(dest)
                .WithCopyObjectSource(
                    new CopySourceObjectArgs()
                        .WithBucket(_bucket)
                        .WithObject(source)),
            ct);
    }

    // =====================================================
    // WATERMARK APPLY
    // =====================================================
    private void ApplyWatermarkInternal(
        IImageProcessingContext ctx,
        Image image)
    {
        if (CachedWatermark == null)
            return;

        using var wm = CachedWatermark.Clone();

        var wmWidth = image.Width / 4;
        var wmHeight = wm.Height * wmWidth / wm.Width;

        wm.Mutate(x => x.Resize(wmWidth, wmHeight));

        var position = new Point(
            (image.Width - wmWidth) / 2,
            (image.Height - wmHeight) / 2);

        ctx.DrawImage(wm, position, 0.5f);
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

    private static string GetContentType(IImageFormat format)
        => format.Name switch
        {
            "JPEG" => "image/jpeg",
            "PNG" => "image/png",
            "WEBP" => "image/webp",
            "BMP" => "image/bmp",
            "TIFF" => "image/tiff",
            _ => "application/octet-stream"
        };

    static Image<Rgba32> LoadSvgAsImage(string path)
    {
        var svg = new SKSvg();

        svg.Load(path);

        var picture = svg.Picture;

        var bounds = picture.CullRect;

        var width = Math.Max(1, (int)bounds.Width);
        var height = Math.Max(1, (int)bounds.Height);

        using var bitmap = new SKBitmap(width, height);

        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.Transparent);

        canvas.DrawPicture(picture);

        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);

        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return Image.Load<Rgba32>(data.AsStream());
    }

    private async Task SaveWithOriginalFormat(
    Image img,
    Stream stream,
    IImageFormat format)
    {
        switch (format.Name)
        {
            case "JPEG":
                await img.SaveAsync(
                    stream,
                    new JpegEncoder
                    {
                        Quality = 100,
                        Interleaved = true
                    });
                break;

            case "PNG":
                await img.SaveAsync(
                    stream,
                    new PngEncoder());
                break;

            case "WEBP":
                await img.SaveAsync(
                    stream,
                    new WebpEncoder
                    {
                        Quality = 100
                    });
                break;

            case "BMP":
                await img.SaveAsync(
                    stream,
                    new BmpEncoder());
                break;

            case "TIFF":
                await img.SaveAsync(
                    stream,
                    new TiffEncoder());
                break;

            default:
                await img.SaveAsPngAsync(stream);
                break;
        }
    }
}