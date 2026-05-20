using Template.Service.Domain.Shared;
using Volo.Abp.AspNetCore.Mvc;

namespace Service.Template.HttpApi;

public abstract class ObjectStorageServiceController : AbpControllerBase
{
    protected ObjectStorageServiceController()
    {
        LocalizationResource = typeof(ObjectStorageServiceResource);
    }
}
