﻿using System.Net;
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

    public async Task<string> GetObjectAsync(
            string objectName,
            string folderName)
    {
        // Create a GetObject request
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectName,
        };

        // Issue request and remember to dispose of the response
        using GetObjectResponse response = await awsS3Client.GetObjectAsync(request);

        var outputPath = $"{folderName}\\{objectName}";

        try
        {
            // Save object to local file
            await response.WriteResponseStreamToFileAsync(outputPath, false, CancellationToken.None);
            return outputPath;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error saving {objectName}: {ex.Message}");
            return null;
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

        PutObjectResponse? response;
        try
        {
            response = await awsS3Client.PutObjectAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PutObjectAsync: {ex.Message}");
            return false;
        }

        if (response != null && response.HttpStatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine($"Success.");
            return true;
        }
        else
        {
            Console.WriteLine($"Could not upload {objectName} to {bucketName}.");
            return false;
        }
    }
}
