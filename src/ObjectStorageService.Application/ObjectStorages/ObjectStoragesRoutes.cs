using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class ObjectStoragesRoutes : BaseRoutes
{
    public ObjectStoragesRoutes(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
        Temp = $"{Default}/temp";
        Finalize = $"{Default}/finalize";
        Promote = $"{Default}/promote";
        GetTempFiles = $"{Default}/temps/get-files";
    }
    public readonly string Temp;
    public readonly string GetTempFiles;
    public readonly string Finalize;
    public readonly string Promote;
}
