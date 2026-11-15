using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class ImageProcessingRoutes : BaseRoutes
{
    public ImageProcessingRoutes(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
        Resize = $"{Default}/resize";
        Compress = $"{Default}/compress";
        Thumbnail = $"{Default}/thumbnail";
        Watermark = $"{Default}/watermark";
    }

    public readonly string Resize;
    public readonly string Compress;
    public readonly string Thumbnail;
    public readonly string Watermark;
}
