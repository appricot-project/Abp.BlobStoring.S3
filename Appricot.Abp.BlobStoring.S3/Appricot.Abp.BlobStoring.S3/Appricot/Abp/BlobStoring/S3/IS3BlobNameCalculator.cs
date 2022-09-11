using Volo.Abp.BlobStoring;

namespace Appricot.Abp.BlobStoring.S3;

public interface IS3BlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}
