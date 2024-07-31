using System.IO.Compression;
using System.Net;
using Amazon.Internal;
using Amazon.S3;
using Amazon.S3.Model;

//
// Amazon S3 Support
//
internal class AWS_S3
{
    private AmazonS3Config? config;
    private AmazonS3Client? awsS3Client;
    private string bucketName;

    public async Task<ListObjectsV2Response?> InitializeAsync(string key, string secret, string bucket, Amazon.RegionEndpoint regionEndpoint)
    {
        bucketName = bucket;

        config = new AmazonS3Config
        {
            RegionEndpoint = regionEndpoint,
            ServiceURL = "https://s3.amazonaws.com",
        };

        awsS3Client = new AmazonS3Client(key, secret, config);

        // attempt to read objects in bucket
        ListObjectsV2Response? listObjects = null;
        try
        {
            listObjects = await awsS3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucket,
            });

        }
        catch
        {
        }

        return listObjects;
    }

    public async Task<bool> GetObjectAsync(
            string objectName,
            string filePath)
    {
        // Create a GetObject request
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectName,
        };

        // Issue request and remember to dispose of the response
        using GetObjectResponse response = await awsS3Client.GetObjectAsync(request);

        try
        {
            // Save object to local file
            await response.WriteResponseStreamToFileAsync($"{filePath}\\{objectName}", true, CancellationToken.None);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error saving {objectName}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PutFileAsync(
           string objectName,
           string filePath)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectName,
            FilePath = filePath,
        };

        var response = await awsS3Client.PutObjectAsync(request);
        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Successfully uploaded {objectName} to {bucketName}.");
            return true;
        }
        else
        {
            Console.WriteLine($"Could not upload {objectName} to {bucketName}.");
            return false;
        }
    }
}
