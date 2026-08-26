namespace ObjectStorageService;

public sealed record ImageFormat(string Name, string MimeType, string Extension)
{
    public static readonly ImageFormat Jpeg = new("JPEG", "image/jpeg", ".jpg");
    public static readonly ImageFormat Png = new("PNG", "image/png", ".png");
    public static readonly ImageFormat Webp = new("WEBP", "image/webp", ".webp");
    public static readonly ImageFormat Gif = new("GIF", "image/gif", ".gif");
    public static readonly ImageFormat Bmp = new("BMP", "image/bmp", ".bmp");
    public static readonly ImageFormat Tiff = new("TIFF", "image/tiff", ".tiff");
    public static readonly ImageFormat Heif = new("HEIF", "image/heif", ".heif");
    public static readonly ImageFormat Avif = new("AVIF", "image/avif", ".avif");
    public static readonly ImageFormat Jxl = new("JXL", "image/jxl", ".jxl");
    public static readonly ImageFormat Svg = new("SVG", "image/svg+xml", ".svg");
    public static readonly ImageFormat Unknown = new("UNKNOWN", "application/octet-stream", "");

    public static ImageFormat FromMimeType(string? mimeType) => mimeType?.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/jpg" => Jpeg,
        "image/png" => Png,
        "image/webp" => Webp,
        "image/gif" => Gif,
        "image/bmp" => Bmp,
        "image/tiff" => Tiff,
        "image/heif" or "image/heic" => Heif,
        "image/avif" => Avif,
        "image/jxl" => Jxl,
        "image/svg+xml" => Svg,
        _ => Unknown
    };

    public static ImageFormat FromLoader(string? loader) => loader?.ToLowerInvariant() switch
    {
        "jpeg" or "jpg" => Jpeg,
        "png" => Png,
        "webp" => Webp,
        "gif" => Gif,
        "bmp" => Bmp,
        "tiff" or "tif" => Tiff,
        "heif" or "heic" => Heif,
        "avif" => Avif,
        "jxl" => Jxl,
        "svg" => Svg,
        _ => Unknown
    };
}
