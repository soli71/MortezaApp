﻿using Minio;
using Minio.DataModel.Args;

namespace OnlineCourse.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;

    public MinioService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task DeleteFileAsync(string bucketName,string objectName)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName));
            Console.WriteLine("File deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }
    }
    public async Task UploadFileAsync(string bucketName, string objectName, string filePath)
    {
        try
        {
            bool bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
                .WithContentType("image/png"));

            Console.WriteLine("File uploaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }
    }
    // Replace the GetFileUrlAsync method with the following code
    public async Task<string> GetFileUrlAsync(string bucketName, string objectName)
    {
        try
        {
            var presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(60 * 60));

            return presignedUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            return null;
        }
    }
}