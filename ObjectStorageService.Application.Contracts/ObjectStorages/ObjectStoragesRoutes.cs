using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class ObjectStoragesRoutes : BaseRoutes
{
    public ObjectStoragesRoutes(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
        Temp = $"{Default}/temp";
        Finalize = $"{Default}/finalize";
        Promote = $"{Default}/promote";
    }
    public readonly string Temp;
    public readonly string Finalize;
    public readonly string Promote;
}
