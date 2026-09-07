using HeyRed.Mime;
using Microsoft.AspNetCore.Http;

namespace ObjectStorageService.ObjectStorages;

public sealed record FileValidationResult(
    bool IsValid,
    string? MimeType = null,
    string? Extension = null,
    string? Error = null,
    object[]? ErrorArgs = null)
{
    public static FileValidationResult Success(
        string mimeType,
        string extension)
        => new(true, mimeType, extension);

    public static FileValidationResult Fail(string error, params object[] args)
        => new(false, Error: error, ErrorArgs: args);
}

public static class FileValidator
{
    private static readonly HashSet<string> ExecutableExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe",
            ".dll",
            ".com",
            ".scr",
            ".msi",
            ".bat",
            ".cmd",
            ".ps1",
            ".psm1",
            ".vbs",
            ".vbe",
            ".js",
            ".jse",
            ".wsf",
            ".wsh",
            ".sh",
            ".bash",
            ".csh",
            ".ksh"
        };

    private static readonly HashSet<string> ExecutableMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/x-dosexec",
            "application/x-executable",
            "application/x-msdownload",
            "application/x-msdos-program",
            "application/x-sh",
            "application/x-shellscript",
            "text/x-shellscript"
        };

    private static readonly Dictionary<string, string[]> MimeExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            ["image/jpeg"] = [".jpg", ".jpeg"],
            ["image/jpg"] = [".jpg", ".jpeg"],
            ["image/pjpeg"] = [".jpg", ".jpeg"],
            ["image/png"] = [".png"],
            ["image/webp"] = [".webp"],
            ["image/gif"] = [".gif"],
            ["image/bmp"] = [".bmp"],
            ["image/x-ms-bmp"] = [".bmp"],
            ["image/tiff"] = [".tif", ".tiff"],
            ["image/svg+xml"] = [".svg"],
            ["image/avif"] = [".avif"],
            ["image/heic"] = [".heic", ".heif"],
            ["image/heif"] = [".heic", ".heif"],

            // Documents
            ["application/pdf"] = [".pdf"],
            ["application/msword"] = [".doc"],
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
                = [".docx"],

            ["application/vnd.ms-excel"] = [".xls"],
            ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]
                = [".xlsx"],

            ["application/vnd.ms-powerpoint"] = [".ppt"],
            ["application/vnd.openxmlformats-officedocument.presentationml.presentation"]
                = [".pptx"],

            // Text
            ["text/plain"] = [".txt"],
            ["text/csv"] = [".csv"],
            ["text/html"] = [".html", ".htm"],
            ["text/css"] = [".css"],
            ["text/javascript"] = [".js"],
            ["application/javascript"] = [".js"],

            // Archives
            ["application/zip"] = [".zip"],
            ["application/x-7z-compressed"] = [".7z"],
            ["application/x-rar-compressed"] = [".rar"],
            ["application/gzip"] = [".gz"],
            ["application/x-tar"] = [".tar"],

            // Audio
            ["audio/mpeg"] = [".mp3"],
            ["audio/wav"] = [".wav"],
            ["audio/ogg"] = [".ogg"],
            ["audio/webm"] = [".webm"],

            // Video
            ["video/mp4"] = [".mp4"],
            ["video/x-msvideo"] = [".avi"],
            ["video/x-matroska"] = [".mkv"],
            ["video/webm"] = [".webm"]
        };

    public static async Task<FileValidationResult> ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length <= 0)
        {
            return FileValidationResult.Fail(FileLocalizationKeys.Empty);
        }

        var extension = NormalizeExtension(
            Path.GetExtension(file.FileName));

        if (string.IsNullOrWhiteSpace(extension))
        {
            return FileValidationResult.Fail(FileLocalizationKeys.ExtensionMissing);
        }

        // 1. Block known executable extensions immediately.
        if (ExecutableExtensions.Contains(extension))
        {
            return FileValidationResult.Fail(
                FileLocalizationKeys.ExecutableExtension,
                extension);
        }

        await using var stream = file.OpenReadStream();

        string? mimeType;

        try
        {
            mimeType = MimeGuesser.GuessMimeType(stream);
        }
        catch
        {
            mimeType = null;
        }

        // 2. If MIME is known, make sure it isn't executable.
        if (!string.IsNullOrWhiteSpace(mimeType) &&
            ExecutableMimeTypes.Contains(mimeType))
        {
            return FileValidationResult.Fail(
                FileLocalizationKeys.ExecutableMime,
                mimeType);
        }

        // 3. Unknown MIME is allowed.
        //
        // This is important for arbitrary files such as:
        // txt, svg, custom formats, etc.
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return FileValidationResult.Success(
                "application/octet-stream",
                extension);
        }

        // 4. If we know the MIME, validate the extension.
        //
        // Some MIME types have multiple valid extensions.
        if (MimeExtensions.TryGetValue(
                mimeType,
                out var validExtensions))
        {
            if (!validExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                return FileValidationResult.Fail(
                    FileLocalizationKeys.ExtensionMimeMismatch,
                    extension,
                    mimeType);
            }
        }

        return FileValidationResult.Success(
            mimeType,
            extension);
    }

    private static string NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        extension = extension
            .Trim()
            .ToLowerInvariant();

        return extension.StartsWith('.')
            ? extension
            : $".{extension}";
    }
}