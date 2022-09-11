using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace Appricot.Abp.BlobStoring.S3;

public class S3CompatibleBlobProvider : BlobProviderBase, ITransientDependency
{
    protected IS3BlobNameCalculator BlobNameCalculator { get; }
    protected IS3ClientFactory S3ClientFactory { get; }
    protected IBlobNormalizeNamingService BlobNormalizeNamingService { get; }

    public S3CompatibleBlobProvider(
        IS3BlobNameCalculator blobNameCalculator,
        IS3ClientFactory s3ClientFactory,
        IBlobNormalizeNamingService blobNormalizeNamingService)
    {
        BlobNameCalculator = blobNameCalculator;
        S3ClientFactory = s3ClientFactory;
        BlobNormalizeNamingService = blobNormalizeNamingService;
    }

    public override async Task<bool> DeleteAsync(BlobProviderDeleteArgs args)
    {
        var blobName = BlobNameCalculator.Calculate(args);
        var containerName = GetContainerName(args);

        using (var amazonS3Client = await GetS3Client(args))
        {
            if (!await BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                return false;
            }

            await amazonS3Client.DeleteObjectAsync(containerName, blobName);

            return true;
        }
    }

    public override async Task<bool> ExistsAsync(BlobProviderExistsArgs args)
    {
        var blobName = BlobNameCalculator.Calculate(args);
        var containerName = GetContainerName(args);

        using (var amazonS3Client = await GetS3Client(args))
        {
            return await BlobExistsAsync(amazonS3Client, containerName, blobName);
        }
    }

    public override async Task<Stream> GetOrNullAsync(BlobProviderGetArgs args)
    {
        var blobName = BlobNameCalculator.Calculate(args);
        var containerName = GetContainerName(args);

        using (var amazonS3Client = await GetS3Client(args))
        {
            if (!await BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                return null;
            }

            var response = await amazonS3Client.GetObjectAsync(containerName, blobName);

            return await TryCopyToMemoryStreamAsync(response.ResponseStream, args.CancellationToken);
        }
    }

    public override async Task SaveAsync(BlobProviderSaveArgs args)
    {
        var blobName = BlobNameCalculator.Calculate(args);
        var configuration = args.Configuration.GetS3Configuration();
        var containerName = GetContainerName(args);

        using (var amazonS3Client = await GetS3Client(args))
        {
            if (!args.OverrideExisting && await BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                throw new BlobAlreadyExistsException(
                    $"Saving BLOB '{args.BlobName}' does already exists in the container '{containerName}'! Set {nameof(args.OverrideExisting)} if it should be overwritten.");
            }

            if (configuration.CreateContainerIfNotExists)
            {
                await CreateContainerIfNotExists(amazonS3Client, containerName);
            }

            await amazonS3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = containerName,
                Key = blobName,
                InputStream = args.BlobStream
            });
        }
    }

    protected virtual async Task<AmazonS3Client> GetS3Client(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetS3Configuration();
        return await S3ClientFactory.GetS3Client(configuration);
    }

    protected virtual async Task<bool> BlobExistsAsync(AmazonS3Client amazonS3Client, string containerName, string blobName)
    {
        // Make sure Blob Container exists.
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3Client, containerName))
        {
            return false;
        }

        try
        {
            await amazonS3Client.GetObjectMetadataAsync(containerName, blobName);
        }
        catch (Exception ex)
        {
            if (ex is AmazonS3Exception)
            {
                return false;
            }

            throw;
        }

        return true;
    }

    protected virtual async Task CreateContainerIfNotExists(AmazonS3Client amazonS3Client, string containerName)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3Client, containerName))
        {
            await amazonS3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = containerName
            });
        }
    }

    protected virtual string GetContainerName(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetS3Configuration();
        return configuration.ContainerName.IsNullOrWhiteSpace()
            ? args.ContainerName
            : BlobNormalizeNamingService.NormalizeContainerName(args.Configuration, configuration.ContainerName);
    }

    protected virtual async Task<Stream> TryCopyToMemoryStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream == null)
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
