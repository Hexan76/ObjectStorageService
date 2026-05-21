using ObjectStorageService.Domain.Shared;
using Template.Service.Domain.Shared;
using Volo.Abp.Application.Services;

namespace ObjectStorageService.Application;

public abstract class ObjectStorageServiceAppService : ApplicationService
{
    protected ObjectStorageServiceAppService()
    {
        LocalizationResource = typeof(ObjectStorageServiceResource);
        ObjectMapperContext = typeof(ObjectStorageServiceApplicationModule);
    }
}
