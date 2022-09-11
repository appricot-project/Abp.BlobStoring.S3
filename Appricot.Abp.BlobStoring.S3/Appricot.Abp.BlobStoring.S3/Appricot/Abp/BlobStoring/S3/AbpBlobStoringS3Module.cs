using Volo.Abp.BlobStoring;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace Appricot.Abp.BlobStoring.S3
{
    [DependsOn(typeof(AbpBlobStoringModule),
        typeof(AbpCachingModule))]
    public class AbpBlobStoringS3Module : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}
