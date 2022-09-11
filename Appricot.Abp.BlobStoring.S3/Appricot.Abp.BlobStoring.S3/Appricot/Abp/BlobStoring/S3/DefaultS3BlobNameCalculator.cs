using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Appricot.Abp.BlobStoring.S3;

public class DefaultS3BlobNameCalculator : IS3BlobNameCalculator, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }

    public DefaultS3BlobNameCalculator(ICurrentTenant currentTenant)
    {
        CurrentTenant = currentTenant;
    }

    public virtual string Calculate(BlobProviderArgs args)
    {
        return CurrentTenant.Id == null
            ? $"host/{args.BlobName}"
            : $"tenants/{CurrentTenant.Id.Value.ToString("D")}/{args.BlobName}";
    }
}
