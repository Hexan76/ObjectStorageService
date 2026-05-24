using Microsoft.Extensions.Options;
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
using Volo.Abp.DependencyInjection;

namespace ObjectStorageService.ObjectStorages.Services;

public class ImageProcessingHelper : IImageProcessingHelper, IScopedDependency
{
    private readonly Image<Rgba32>? _cachedWatermark;

    public ImageProcessingHelper(IOptions<ObjectStorageOptions> options)
    {
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
        using var image = await Image.LoadAsync(input, ct);
        var format = image.Metadata.DecodedImageFormat;

        image.Mutate(x =>
        {
            x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(width, height)
            });
        });

        var outStream = new MemoryStream();
        await SaveWithOriginalFormat(image, outStream, format);
        outStream.Position = 0;
        return (outStream, format);
    }

    public async Task<(MemoryStream Stream, IImageFormat Format)> ApplyWatermarkAsync(Stream input, CancellationToken ct)
    {
        using var image = await Image.LoadAsync(input, ct);
        var format = image.Metadata.DecodedImageFormat;

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

    private async Task SaveWithOriginalFormat(Image img, Stream stream, IImageFormat format)
    {
        switch (format.Name)
        {
            case "JPEG":
                await img.SaveAsync(stream, new JpegEncoder { Quality = 100, Interleaved = true });
                break;
            case "PNG":
                await img.SaveAsync(stream, new PngEncoder());
                break;
            case "WEBP":
                await img.SaveAsync(stream, new WebpEncoder { Quality = 100 });
                break;
            case "BMP":
                await img.SaveAsync(stream, new BmpEncoder());
                break;
            case "TIFF":
                await img.SaveAsync(stream, new TiffEncoder());
                break;
            default:
                await img.SaveAsPngAsync(stream);
                break;
        }
    }
}
