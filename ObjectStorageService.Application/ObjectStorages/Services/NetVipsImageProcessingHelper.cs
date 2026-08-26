using Microsoft.Extensions.Options;
using NetVips;
using SixLabors.ImageSharp.Formats;
using Volo.Abp.DependencyInjection;

namespace ObjectStorageService.ObjectStorages.Services;

[ExposeServices(typeof(INetImageProcessingHelper))]
public sealed class NetVipsImageProcessingHelper : INetImageProcessingHelper, IDisposable, IScopedDependency
{
    private const int DefaultTargetSize = 50 * 1024;
    private readonly Image? _cachedWatermark;
    private readonly bool _isCompress;
    private readonly string _compressMimeType;
    private readonly int _defaultCompressionTargetSize;
    private readonly int _thumbnailTargetSize;

    public NetVipsImageProcessingHelper(IOptions<ObjectStorageOptions> options)
    {
        var value = options.Value;
        _isCompress = value.IsCompressWebp;
        _compressMimeType = value.CompressMimeType;
        _defaultCompressionTargetSize = value.DefaultCompressionTargetSize > 0 ? value.DefaultCompressionTargetSize : DefaultTargetSize;
        _thumbnailTargetSize = value.ThumbnailTargetSize > 0 ? value.ThumbnailTargetSize : 10 * 1024;

        if (!string.IsNullOrWhiteSpace(value.SvgFileFullName) && File.Exists(value.SvgFileFullName))
        {
            try
            {
                _cachedWatermark = Image.NewFromFile(value.SvgFileFullName, access: Enums.Access.Sequential);
            }
            catch
            {
                _cachedWatermark = null;
            }
        }
    }

    public async Task<(MemoryStream Stream, ImageFormat Format)> ResizeAsync(Stream input, int width, int height, CancellationToken ct = default)
    {
        ValidateSize(width, height);
        ct.ThrowIfCancellationRequested();

        if (_isCompress)
        {
            var compressed = await CompressToWebp(input, _defaultCompressionTargetSize, ct);
            using var compressedImage = Load(compressed.Stream);
            var resized = ResizeIfNeeded(compressedImage, width, height);
            return await WriteAsync(resized, ImageFormat.FromMimeType(_compressMimeType), 90, ct);
        }

        using var image = Load(input);
        var format = DetectFormat(image);
        var resizedImage = ResizeIfNeeded(image, width, height);
        return await WriteAsync(resizedImage, format, 90, ct);
    }

    public async Task<(MemoryStream Stream, ImageFormat Format)> Thumbnail(Stream input, int width, int height, CancellationToken ct = default)
    {
        ValidateSize(width, height);
        ct.ThrowIfCancellationRequested();

        using var image = Load(input);
        using var oriented = image.Autorot();

        var scale = Math.Min((double)width / oriented.Width, (double)height / oriented.Height);
        scale = Math.Min(scale, 1.0);
        using var resized = oriented.Resize(scale, Enums.Kernel.Lanczos3);

        var qualities = new[] { 35, 25, 18, 12 };
        MemoryStream? best = null;

        try
        {
            foreach (var quality in qualities)
            {
                ct.ThrowIfCancellationRequested();
                var bytes = resized.WriteToBuffer($".webp[Q={quality},smart_subsample=true,strip=true]");
                var candidate = new MemoryStream(bytes, writable: false);

                if (best is null || candidate.Length < best.Length)
                {
                    best?.Dispose();
                    best = candidate;
                }
                else
                {
                    candidate.Dispose();
                }

                if (best.Length <= _thumbnailTargetSize)
                {
                    best.Position = 0;
                    return (best, ImageFormat.Webp);
                }
            }
            using var smaller = oriented.Resize(scale, Enums.Kernel.Lanczos3);
            //using var smaller = resized.Resize(0.92, kwargs: new VOption { { "kernel", Enums.Kernel.Lanczos3 } });
            var smallerBytes = smaller.WriteToBuffer(".webp[Q=12,smart_subsample=true,strip=true]");
            var smallerStream = new MemoryStream(smallerBytes, writable: false);

            if (best is null || smallerStream.Length < best.Length)
            {
                best?.Dispose();
                best = smallerStream;
            }
            else
            {
                smallerStream.Dispose();
            }

            best.Position = 0;
            return (best, ImageFormat.Webp);
        }
        catch
        {
            best?.Dispose();
            throw;
        }
    }

    public async Task<(MemoryStream Stream, ImageFormat Format)> ApplyWatermarkAsync(Stream input, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (_isCompress)
        {
            var compressed = await CompressToWebp(input, _defaultCompressionTargetSize, ct);
            using var compressedImage = Load(compressed.Stream);
            using var watermarked = ApplyWatermark(compressedImage);
            return await WriteAsync(watermarked, ImageFormat.FromMimeType(_compressMimeType), 90, ct);
        }

        using var image = Load(input);
        var format = DetectFormat(image);
        using var result = ApplyWatermark(image);
        return await WriteAsync(result, format, 90, ct);
    }

    public async Task<(MemoryStream Stream, ImageFormat Format)> CompressToWebp(Stream input, int targetSize = DefaultTargetSize, CancellationToken ct = default)
    {
        if (targetSize <= 0)
            targetSize = _defaultCompressionTargetSize;

        ct.ThrowIfCancellationRequested();
        using var image = Load(input);
        using var oriented = image.Autorot();
        using var current = ShrinkIfNeeded(oriented, targetSize);

        MemoryStream? best = null;
        var qualities = new[] { 80, 65, 50, 35, 25 };

        try
        {
            foreach (var quality in qualities)
            {
                ct.ThrowIfCancellationRequested();
                var bytes = current.WriteToBuffer($".webp[Q={quality},smart_subsample=true]");
                var candidate = new MemoryStream(bytes, writable: false);

                if (best is null || candidate.Length < best.Length)
                {
                    best?.Dispose();
                    best = candidate;
                }
                else
                {
                    candidate.Dispose();
                }

                if (best.Length <= targetSize)
                {
                    best.Position = 0;
                    return (best, ImageFormat.Webp);
                }
            }

            best!.Position = 0;
            return (best, ImageFormat.Webp);
        }
        catch
        {
            best?.Dispose();
            throw;
        }
    }

    public async Task<(MemoryStream Stream, ImageFormat Format)> LoadOrginal(Stream input, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var image = Load(input);
        var format = DetectFormat(image);
        return await WriteAsync(image, format, 100, ct);
    }

    public string GetContentType(ImageFormat format) => format.MimeType;

    private Image Load(Stream input)
    {
        if (input.CanSeek)
            input.Position = 0;

        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        return Image.NewFromBuffer(buffer.ToArray(), kwargs: new VOption
        {
            { "access", Enums.Access.Sequential },
            { "fail", true }
        });
    }

    private static async Task<byte[]> ReadBytesAsync(Stream input, CancellationToken ct)
    {
        if (input.CanSeek)
            input.Position = 0;

        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    private static Image ResizeIfNeeded(Image image, int width, int height)
    {
        if (image.Width <= width && image.Height <= height)
            return image.Copy();

        var scale = Math.Min((double)width / image.Width, (double)height / image.Height);
        //return image.Resize(scale, kwargs: new VOption { { "kernel", Enums.Kernel.Lanczos3 } });
        return image.Resize(scale, Enums.Kernel.Lanczos3);
    }

    private static Image ShrinkIfNeeded(Image image, int targetSize)
    {
        var pixels = (double)image.Width * image.Height;
        var expectedBytes = pixels / 7d;

        if (expectedBytes <= targetSize)
            return image.Copy();

        var scale = Math.Clamp(Math.Sqrt(targetSize / expectedBytes), 0.2, 1.0);
        return image.Resize(scale, Enums.Kernel.Lanczos3);
    }

    private Image ApplyWatermark(Image image)
    {
        if (_cachedWatermark is null)
            return image.Copy();

        var wmWidth = Math.Max(1, image.Width / 4);
        var wmScale = (double)wmWidth / _cachedWatermark.Width;

        using var resized = _cachedWatermark.Resize(
            wmScale,
            Enums.Kernel.Lanczos3);

        using var rgba = resized.Bands == 4
            ? resized.Copy()
            : resized.AddAlpha();

        using var alpha = rgba[3] * 0.5;

        using var rgb = rgba.ExtractBand(0, n: 3);
        using var watermark = rgb.Bandjoin(alpha);

        var x = Math.Max(0, (image.Width - watermark.Width) / 2);
        var y = Math.Max(0, (image.Height - watermark.Height) / 2);

        return image.Composite2(
            watermark,
            Enums.BlendMode.Over,
            x,
            y);
    }
    private static async Task<(MemoryStream Stream, ImageFormat Format)> WriteAsync(Image image, ImageFormat format, int quality, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var suffix = GetWriteSuffix(format, quality);
        var bytes = image.WriteToBuffer(suffix);
        await Task.CompletedTask;
        ct.ThrowIfCancellationRequested();
        return (new MemoryStream(bytes, writable: false), format);
    }

    private static string GetWriteSuffix(ImageFormat format, int quality) => format switch
    {
        var f when f == ImageFormat.Jpeg => $".jpg[Q={quality},strip=true]",
        var f when f == ImageFormat.Png => ".png[strip=true,compression=6]",
        var f when f == ImageFormat.Webp => $".webp[Q={quality},strip=true]",
        var f when f == ImageFormat.Gif => ".gif",
        var f when f == ImageFormat.Bmp => ".bmp",
        var f when f == ImageFormat.Tiff => ".tif[compression=jpeg]",
        var f when f == ImageFormat.Avif => $".avif[Q={quality}]",
        var f when f == ImageFormat.Heif => $".heif[Q={quality}]",
        var f when f == ImageFormat.Jxl => $".jxl[Q={quality}]",
        _ => ".png"
    };

    private static ImageFormat DetectFormat(Image image)
    {
        var loader = image.Get("vips-loader") as string;
        if (loader is null)
            return ImageFormat.Unknown;

        var normalized = loader.ToLowerInvariant();
        if (normalized.Contains("jpeg")) return ImageFormat.Jpeg;
        if (normalized.Contains("png")) return ImageFormat.Png;
        if (normalized.Contains("webp")) return ImageFormat.Webp;
        if (normalized.Contains("gif")) return ImageFormat.Gif;
        if (normalized.Contains("bmp")) return ImageFormat.Bmp;
        if (normalized.Contains("tiff")) return ImageFormat.Tiff;
        if (normalized.Contains("heif")) return ImageFormat.Heif;
        if (normalized.Contains("avif")) return ImageFormat.Avif;
        if (normalized.Contains("jxl")) return ImageFormat.Jxl;
        if (normalized.Contains("svg")) return ImageFormat.Svg;
        return ImageFormat.Unknown;
    }

    private static void ValidateSize(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException($"{nameof(width)}/{nameof(height)}", "Image dimensions must be greater than zero.");
    }

    public void Dispose() => _cachedWatermark?.Dispose();
}
