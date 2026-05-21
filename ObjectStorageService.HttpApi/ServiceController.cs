using Template.Service.Domain.Shared;
using Volo.Abp.AspNetCore.Mvc;

namespace ObjectStorageService.HttpApi;

public abstract class ObjectStorageServiceController : AbpControllerBase
{
    protected ObjectStorageServiceController()
    {
        LocalizationResource = typeof(ObjectStorageServiceResource);
    }
}
