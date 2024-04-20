using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.Runtime.Internal;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

Console.WriteLine("Iniciando....");

var chain = new CredentialProfileStoreChain();
AWSCredentials awsCredentials;
if (chain.TryGetAWSCredentials("default", out awsCredentials))
{
    // Use awsCredentials to create an Amazon S3 service client
    using (var client = new AmazonS3Client(awsCredentials))
    {
        var response = await client.ListBucketsAsync();
        Console.WriteLine($"Number of buckets: {response.Buckets.Count}");
    }
}


var bucketName = "edu007-storage";
var keyPrefix = "19-04-2024";
var uploadPath = "C:\\Users\\eduar\\Desktop\\UploadToBucket";

using (var client = new AmazonS3Client(awsCredentials))
{
    var transferUtil = new TransferUtility(client);
    var success = await UploadFullDirectoryAsync(transferUtil, bucketName, keyPrefix, uploadPath);
    if (success)
    {
        Console.WriteLine($"Successfully uploaded the files in {uploadPath} to {bucketName}.");
        Console.WriteLine($"{bucketName} currently contains the following files:");
        await DisplayBucketFiles(client, bucketName, keyPrefix);
        Console.WriteLine();
    }
}


Console.ReadKey();

static async Task<bool> UploadFullDirectoryAsync(
    TransferUtility transferUtil,
    string bucketName,
    string keyPrefix,
    string localPath)
{
    if (Directory.Exists(localPath))
    {
        try
        {
            await transferUtil.UploadDirectoryAsync(new TransferUtilityUploadDirectoryRequest
            {
                BucketName = bucketName,
                KeyPrefix = keyPrefix,
                Directory = localPath,
            });

            return true;
        }
        catch (AmazonS3Exception s3Ex)
        {
            Console.WriteLine($"Can't upload the contents of {localPath} because:");
            Console.WriteLine(s3Ex?.Message);
            return false;
        }
    }
    else
    {
        Console.WriteLine($"The directory {localPath} does not exist.");
        return false;
    }
}

static async Task DisplayBucketFiles(IAmazonS3 client, string bucketName, string s3Path)
{
    ListObjectsV2Request request = new()
    {
        BucketName = bucketName,
        Prefix = s3Path,
        MaxKeys = 5,
    };

    var response = new ListObjectsV2Response();

    do
    {
        response = await client.ListObjectsV2Async(request);

        response.S3Objects
            .ForEach(obj => Console.WriteLine($"{obj.Key}"));

        // If the response is truncated, set the request ContinuationToken
        // from the NextContinuationToken property of the response.
        request.ContinuationToken = response.NextContinuationToken;
    } while (response.IsTruncated);
}



