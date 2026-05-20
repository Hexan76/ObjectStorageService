using Service.Template.Domain.Shared;
using Template.Service.Domain.Shared;
using Volo.Abp.Application.Services;

namespace Service.Template.Application;

public abstract class ObjectStorageServiceAppService : ApplicationService
{
    protected ObjectStorageServiceAppService()
    {
        LocalizationResource = typeof(ObjectStorageServiceResource);
        ObjectMapperContext = typeof(ObjectStorageServiceApplicationModule);
    }
}
