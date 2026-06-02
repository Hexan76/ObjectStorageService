using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Svg.Skia;
using Volo.Abp.DependencyInjection;

namespace ObjectStorageService.ObjectStorages.Services;

public class ImageProcessingHelper : IImageProcessingHelper, IScopedDependency
{
    private readonly Image<Rgba32>? _cachedWatermark;
    private readonly bool _isCompress;
    private readonly string _compressMimeType;

    private const int DefaultTargetSize = 50 * 1024;

    public ImageProcessingHelper(IOptions<ObjectStorageOptions> options)
    {
        _isCompress = options.Value.IsCompressWebp;
        _compressMimeType = options.Value.CompressMimeType;
        try
        {
            _cachedWatermark = LoadSvgAsImage(options.Value.SvgFileFullName);
        }
        catch
        {
            _cachedWatermark = null;
        }
    }

    public async Task<(MemoryStream Stream, IImageFormat Format)> ResizeAsync(Stream input, int width, int height, CancellationToken ct)
    {
        if (input.CanSeek)
            input.Position = 0;

        Stream workingStream = input;
        IImageFormat? format = null;

        if (_isCompress)
        {

            var resultCompress = await CompressToWebp(input, width * height, ct);
            workingStream = resultCompress.Stream;
            format = MimeToExtension(_compressMimeType);
        }

        if (workingStream.CanSeek)
            workingStream.Position = 0;

        using var image = await Image.LoadAsync(workingStream, ct);
        format ??= image.Metadata.DecodedImageFormat;

        image.Mutate(x =>
        {
            x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(width, height)
            });
        });

        var outStream = new MemoryStream();
        await SaveWithOriginalFormat(image, outStream, format, quality: 90);
        outStream.Position = 0;
        return (outStream, format);
    }
    public async Task<(MemoryStream Stream, IImageFormat Format)> Thumbnail(
            Stream input,
            int width,
            int height,
            CancellationToken ct)
    {
        if (input.CanSeek)
            input.Position = 0;

        using var image =
            await Image.LoadAsync(input, ct);

        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;

        image.Mutate(x =>
        {
            x.AutoOrient();

            x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),

                Mode = ResizeMode.Max,

                // smaller files
                Sampler =
                    KnownResamplers.Box,

                Compand = false
            });
        });

        const int target =
            10 * 1024;

        int[] qualities =
            [35, 25, 18, 12];

        double scale = 1.0;

        MemoryStream? best =
            null;

        while (true)
        {
            foreach (var quality in qualities)
            {
                var ms =
                    new MemoryStream();

                await image.SaveAsync(
                    ms,
                    new WebpEncoder
                    {
                        Quality = quality,
                        Method =
                            WebpEncodingMethod.Fastest,
                        FileFormat =
                            WebpFileFormatType.Lossy
                    },
                    ct);

                if (
                    best == null
                    || ms.Length < best.Length)
                {
                    best?.Dispose();
                    best = ms;
                }
                else
                {
                    ms.Dispose();
                }

                if (
                    best.Length <= target)
                {
                    best.Position = 0;

                    return (
                        best,
                        WebpFormat.Instance
                    );
                }
            }

            scale *= 0.92;

            int nextWidth =
                (int)(image.Width * 0.92);

            int nextHeight =
                (int)(image.Height * 0.92);

            if (
                nextWidth < 120
                || nextHeight < 120)
            {
                break;
            }

            image.Mutate(x =>
                x.Resize(
                    nextWidth,
                    nextHeight,
                    KnownResamplers.Box));
        }

        best!.Position = 0;

        return (
            best,
            WebpFormat.Instance
        );
    }
    public async Task<(MemoryStream Stream, IImageFormat Format)> ApplyWatermarkAsync(Stream input, CancellationToken ct)
    {
        if (input.CanSeek)
            input.Position = 0;

        Stream workingStream = input;
        IImageFormat? format = null;

        Stream compressStream;
        if (_isCompress)
        {

            var resultCompress = await CompressToWebp(input, ct: ct);
            workingStream = resultCompress.Stream;
            format = MimeToExtension(_compressMimeType);
            format = MimeToExtension(_compressMimeType);
        }

        if (workingStream.CanSeek)
            workingStream.Position = 0;

        using var image = await Image.LoadAsync(workingStream, ct);
        format ??= image.Metadata.DecodedImageFormat;

        image.Mutate(ctx =>
        {
            ApplyWatermarkInternal(ctx, image);
        });

        var outStream = new MemoryStream();
        await SaveWithOriginalFormat(image, outStream, format);
        outStream.Position = 0;
        return (outStream, format);
    }

    public string GetContentType(IImageFormat format)
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

    private void ApplyWatermarkInternal(IImageProcessingContext ctx, Image image)
    {
        if (_cachedWatermark == null)
            return;

        using var wm = _cachedWatermark.Clone();

        var wmWidth = image.Width / 4;
        var wmHeight = wm.Height * wmWidth / wm.Width;

        wm.Mutate(x => x.Resize(wmWidth, wmHeight));

        var position = new Point((image.Width - wmWidth) / 2, (image.Height - wmHeight) / 2);

        ctx.DrawImage(wm, position, 0.5f);
    }

    private async Task SaveWithOriginalFormat(
        Image img,
        Stream stream,
        IImageFormat format,
        int quality = 100)
    {
        switch (format)
        {
            case JpegFormat:
                await img.SaveAsync(stream,
                    new JpegEncoder
                    {
                        Quality = quality
                    });
                break;

            case PngFormat:
                await img.SaveAsync(stream,
                    new PngEncoder());
                break;

            case WebpFormat:
                await img.SaveAsync(stream,
                    new WebpEncoder
                    {
                        Quality = quality
                    });
                break;

            case BmpFormat:
                await img.SaveAsync(stream,
                    new BmpEncoder());
                break;

            case TiffFormat:
                await img.SaveAsync(stream,
                    new TiffEncoder());
                break;

            default:
                await img.SaveAsPngAsync(stream);
                break;
        }
    }

    public async Task<(MemoryStream Stream, IImageFormat Format)>
        CompressToWebp(
            Stream input,
            int targetSize = DefaultTargetSize,
            CancellationToken ct = default)
    {
        if (input.CanSeek)
            input.Position = 0;

        using var image =
            await Image.LoadAsync(input, ct);

        image.Mutate(x => x.AutoOrient());

        using var current =
            image.CloneAs<Rgba32>();

        // step 1: initial aggressive resize (important)
        ShrinkIfNeeded(current, targetSize);

        MemoryStream? best = null;

        // step 2: single-pass quality cascade (NO resize loop)
        int[] qualities = [80, 65, 50, 35, 25];

        foreach (var q in qualities)
        {
            var ms = new MemoryStream();

            await current.SaveAsync(
                ms,
                new WebpEncoder
                {
                    Quality = q,
                    Method = WebpEncodingMethod.Fastest
                },
                ct);

            if (best == null || ms.Length < best.Length)
            {
                if (best != null)
                    await best.DisposeAsync();

                best = ms;
            }
            else
            {
                await ms.DisposeAsync();
            }

            if (best.Length <= targetSize)
            {
                best.Position = 0;
                return (best, WebpFormat.Instance);
            }
        }

        best!.Position = 0;
        return (best, WebpFormat.Instance);
    }
    private void ShrinkIfNeeded(Image<Rgba32> img, int targetSize)
    {
        long pixels = img.Width * img.Height;
        long expectedBytes = pixels / 7; 

        if (expectedBytes <= targetSize)
            return;

        double scale = Math.Sqrt((double)targetSize / expectedBytes);

        scale = Math.Clamp(scale, 0.2, 1.0);

        img.Mutate(x =>
            x.Resize(
                (int)(img.Width * scale),
                (int)(img.Height * scale),
                KnownResamplers.Box));
    }
    public async Task<(MemoryStream Stream, IImageFormat Format)> LoadOrginal(Stream input, CancellationToken ct)
    {
        if (input.CanSeek)
            input.Position = 0;

        Stream workingStream = input;
        IImageFormat? format = null;

        Stream compressStream;

        if (workingStream.CanSeek)
            workingStream.Position = 0;

        using var image = await Image.LoadAsync(workingStream, ct);
        format = image.Metadata.DecodedImageFormat;

        var outStream = new MemoryStream();
        await SaveWithOriginalFormat(image, outStream, format);
        outStream.Position = 0;
        return (outStream, format);
    }

    private static IImageFormat MimeToExtension(string mime)
    => mime switch
    {
        "image/jpeg" => JpegFormat.Instance,
        "image/png" => PngFormat.Instance,
        "image/webp" => WebpFormat.Instance,
        "image/gif" => GifFormat.Instance,
        _ => JpegFormat.Instance
    };
}
