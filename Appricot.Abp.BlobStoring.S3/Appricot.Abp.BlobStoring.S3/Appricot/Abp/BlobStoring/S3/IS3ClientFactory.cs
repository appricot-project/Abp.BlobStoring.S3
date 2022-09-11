using Amazon.S3;
using System.Threading.Tasks;

namespace Appricot.Abp.BlobStoring.S3;

public interface IS3ClientFactory
{
    Task<AmazonS3Client> GetS3Client(S3BlobProviderConfiguration configuration);
}
