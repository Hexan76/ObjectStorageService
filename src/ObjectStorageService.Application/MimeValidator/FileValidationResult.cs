namespace ObjectStorageService.ObjectStorages.Validation;

public sealed record FileValidationResult(
    bool IsValid,
    string? MimeType,
    string? Extension,
    string? Error)
{
    public static FileValidationResult Success(
        string mimeType,
        string extension)
        => new(
            true,
            mimeType,
            extension,
            null);

    public static FileValidationResult Fail(
        string error)
        => new(
            false,
            null,
            null,
            error);
}