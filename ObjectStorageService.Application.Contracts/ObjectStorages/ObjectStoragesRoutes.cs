using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class ObjectStoragesRoutes : BaseRoutes
{
    public ObjectStoragesRoutes(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
        Temp = $"{Default}/temp";
        Finalize = $"{Default}/finalize";
    }
    public readonly string Temp;
    public readonly string Finalize;
}
