using System;
using Volo.Abp.BlobStoring;

namespace Appricot.Abp.BlobStoring.S3;

public static class S3CompatibleBlobContainerConfigurationExtensions
{
    public static S3BlobProviderConfiguration GetS3Configuration(
        this BlobContainerConfiguration containerConfiguration)
    {
        return new S3BlobProviderConfiguration(containerConfiguration);
    }

    public static BlobContainerConfiguration UseS3Storage(
        this BlobContainerConfiguration containerConfiguration,
        Action<S3BlobProviderConfiguration> s3ConfigureAction)
    {
        containerConfiguration.ProviderType = typeof(S3CompatibleBlobProvider);
        containerConfiguration.NamingNormalizers.TryAdd<S3BlobNamingNormalizer>();

        s3ConfigureAction(new S3BlobProviderConfiguration(containerConfiguration));

        return containerConfiguration;
    }
}
