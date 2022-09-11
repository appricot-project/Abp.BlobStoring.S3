using Amazon.S3;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Appricot.Abp.BlobStoring.S3;

public class DefaultS3ClientFactory : IS3ClientFactory, ITransientDependency
{
    public virtual Task<AmazonS3Client> GetS3Client(
        S3BlobProviderConfiguration configuration)
    {
        return Task.FromResult(
            new AmazonS3Client(
                configuration.AccessKeyId,
                configuration.SecretAccessKey,
                GetS3Configuration(configuration)));
    }

    protected virtual AmazonS3Config GetS3Configuration(
        S3BlobProviderConfiguration configuration)
    {
        if (configuration.ServiceURL.IsNullOrWhiteSpace())
        {
            return null;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = configuration.ServiceURL
        };

        return config;
    }

}
