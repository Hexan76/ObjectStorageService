namespace ObjectStorageService.Domain;

public static class ObjectStorageServiceDbProperties
{
    public static string DbTablePrefix { get; set; } = "Service";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Write";
}
