namespace ObjectStorageService.Application.Contracts;
public sealed record ProcessedImage(
    MemoryStream Stream,
    string ContentType,
    string Extension);