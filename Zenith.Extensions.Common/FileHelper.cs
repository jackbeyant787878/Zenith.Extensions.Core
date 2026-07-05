
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Azure.Storage.Blobs;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using Zenith.Extensions.Utils;
namespace Zenith.Extensions.Common
{
    public static class FileHelper
    {
        /// <summary>
        /// 将指定的文件生成zip格式的压缩文件
        /// </summary>
        /// <param name="fileToZipFilePath">要压缩的文件全路径</param>
        /// <param name="zipedFileFullPath">压缩后的文件全路径</param>
        /// <param name="password">压缩密码</param>
        /// <returns></returns>
        public static bool ZipFile(string fileToZipFilePath, string zipedFileFullPath, string password = null)
        {
            bool result = true;
            ZipOutputStream zipStream = null;
            FileStream fs = null;
            ZipEntry ent = null;

            if (!File.Exists(fileToZipFilePath))
                return false;

            try
            {
                fs = File.OpenRead(fileToZipFilePath);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                fs = File.Create(zipedFileFullPath);
                zipStream = new ZipOutputStream(fs);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                ent = new ZipEntry(Path.GetFileName(fileToZipFilePath));
                zipStream.PutNextEntry(ent);
                zipStream.SetLevel(9);
                zipStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
                if (ent != null)
                {
                    ent = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            GC.Collect();
            GC.Collect(1);

            return result;
        }

        /// <summary>
        /// 将指定的文件生成gz格式的压缩文件
        /// </summary>
        /// <param name="fileToZipFilePath">要压缩的文件全路径</param>
        /// <param name="zipedFileFullPath">压缩后的文件全路径</param>
        public static void GZipFile(string fileToZipFilePath, string zipedFileFullPath)
        {
            Stream s = new GZipOutputStream(File.Create(zipedFileFullPath));
            FileStream fs = File.OpenRead(fileToZipFilePath);
            int size;
            byte[] buf = new byte[4096];
            do
            {
                size = fs.Read(buf, 0, buf.Length);
                s.Write(buf, 0, size);
            } while (size > 0);
            s.Close();
            fs.Close();
        }

        /// <summary>
        /// 将指定的文件上传到谷歌云
        /// </summary>
        /// <param name="toUploadedFileFullPath">需上传文件的全路径</param>
        /// <param name="fileFullName">文件全名(包含后缀名)</param>
        /// <param name="googleCloudStoragePrivateKeyJson">谷歌云私钥Json</param>
        /// <param name="cloudStorageBucket">指定的Bucket</param>
        /// <param name="jobLog">日志log</param>
        /// <returns></returns>
        public static bool UploadFileToGoogleCloudStorage(string toUploadedFileFullPath, string fileFullName, string googleCloudStoragePrivateKeyJson,
            string cloudStorageBucket, ScheduledJobLog jobLog)
        {

            if (!string.IsNullOrEmpty(googleCloudStoragePrivateKeyJson))
            {

                jobLog.AppendDebugInfo($"GoogleCloudStorage start.");

                try
                {
                    var credential = GoogleCredential.FromJson(googleCloudStoragePrivateKeyJson);
                    var client = StorageClient.Create(credential);
                    using (var memoryStream = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(toUploadedFileFullPath, FileMode.Open, FileAccess.Read))
                            fs.CopyTo(memoryStream);

                        var folder = string.Empty;
                        var googleCloudStorageBucketArray = cloudStorageBucket.Split('/');
                        var googleCloudStorageBucket = googleCloudStorageBucketArray[0];
                        if (googleCloudStorageBucketArray.Length > 1)
                        {
                            for (int i = 0; i < googleCloudStorageBucketArray.Length; i++)
                            {
                                if (i == 0)
                                {
                                    continue;
                                }
                                folder += googleCloudStorageBucketArray[i] + "/";
                            }
                        }

                        var dataObject = client.UploadObject(googleCloudStorageBucket, $"{folder}{fileFullName}", null, memoryStream);
                        jobLog.AppendDebugInfo($"GoogleCloudStorage MediaLink:" + dataObject.MediaLink);
                    }
                }
                catch (Exception ex)
                {
                    jobLog.AppendDebugInfo($"GoogleCloudStorage exception msg:" + ex.Message);
                    jobLog.AppendDebugInfo($"GoogleCloudStorage exception stackTrace:" + ex.StackTrace);
                    return false;
                }

                jobLog.AppendDebugInfo($"GoogleCloudStorage end.");
            }

            return true;
        }

        /// <summary>
        /// 检查谷歌云的认证和Bucket是否存在
        /// </summary>
        /// <param name="googleCloudStoragePrivateKeyJson"></param>
        /// <param name="cloudStorageBucket"></param>
        /// <returns></returns>
        public static (bool IsSuccess, string message) CheckGCSPrivateKeyJsonAndBucket(string googleCloudStoragePrivateKeyJson,
          string cloudStorageBucket)
        {
            try
            {
                var credential = GoogleCredential.FromJson(googleCloudStoragePrivateKeyJson);
                var client = StorageClient.Create(credential);
                var googleCloudStorageBucketArray = cloudStorageBucket.Split('/');
                var googleCloudStorageBucket = googleCloudStorageBucketArray[0];
                var bucket = client.GetBucket(googleCloudStorageBucket);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Error deserializing JSON credential data.")
                {
                    return (false, "Private key JSON is wrong");
                }
                else if (ex.Message.Contains("The specified bucket does not exist") ||
                    ex.Message.Contains("does not have storage.buckets.get access to the Google Cloud Storage bucket. [403]"))
                {
                    return (false, "Google bucket is not found");
                }
                else if (ex.Message.Contains("Bucket names must be at least 3 characters in length"))
                {
                    return (false, "Bucket names must be at least 3 characters in length");
                }
                else
                {
                    return (false, "");
                }
            }

            return (true, "");

        }

        /// <summary>
        /// upload file to  Microsoft Azure Blob Storage
        /// </summary>
        /// <param name="toUploadedFileFullPath">需上传文件的全路径</param>
        /// <param name="connectionString">Storage Account 连接串</param>
        /// <param name="containerName,">specified container</param>
        /// <param name="jobLog">日志log</param>
        /// <returns></returns>
        public static bool UploadFileToMicrosoftAzureBlobStorage(string toUploadedFileFullPath, string fileName, string connectionString,
            string containerName, ScheduledJobLog jobLog)
        {

            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(toUploadedFileFullPath)
                && !string.IsNullOrEmpty(containerName) && !string.IsNullOrEmpty(fileName))
            {
                jobLog.AppendDebugInfo($"MicrosoftAzureBlobStorage start.");
                try
                {
                    BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
                    using (var stream = new MemoryStream(File.ReadAllBytes(toUploadedFileFullPath)))
                    {
                        var blobClient = containerClient.GetBlobClient(fileName);
                        blobClient.Upload(stream, overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    jobLog.AppendDebugInfo($"MicrosoftAzureBlobStorage exception msg:" + ex.Message);
                    jobLog.AppendDebugInfo($"MicrosoftAzureBlobStorage exception stackTrace:" + ex.StackTrace);
                    return false;
                }

                jobLog.AppendDebugInfo($"MicrosoftAzureBlobStorage end.");
            }

            return true;
        }

        /// <summary>
        /// 检验微软云的connectionString和containerName是否有效
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static (bool IsSuccess, string message) CheckMicrosoftAzureInfo(string connectionString,string containerName)
        {
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
                var response = containerClient.Exists();
                if (!response.Value)
                {
                    return (false, "Microsoft Azure Container is not found");
                }
            }

            catch (FormatException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception e) {

                return (false, "Connection string is not correct");
            }
            return (true,"");
        }

        /// <summary>
        /// 校验Amazon S3上传时的信息
        /// </summary>
        /// <param name="S3RegionEndpoint"></param>
        /// <param name="S3Accesskey"></param>
        /// <param name="S3Secretkey"></param>
        /// <param name="S3Bucket"></param>
        /// <returns></returns>

        public static (bool IsSuccess, string message) CheckAWSAmazonS3Info(string S3RegionEndpoint, string S3Accesskey,
            string S3Secretkey, string S3Bucket)
        {

            if (!string.IsNullOrEmpty(S3RegionEndpoint) && !string.IsNullOrEmpty(S3Accesskey)
                && !string.IsNullOrEmpty(S3Secretkey))
            {
                try
                {
                    var fieldInfo = typeof(RegionEndpoint).GetField(S3RegionEndpoint);
                    var region = (fieldInfo.GetValue(typeof(RegionEndpoint)) as RegionEndpoint);
                    var client = new AmazonS3Client(S3Accesskey, S3Secretkey, region);
                    var result = client.GetBucketLocationAsync(S3Bucket).Result;
                    if (result.Location.Value.ToString() != region.SystemName)
                    {
                        return (false, "The bucket you are attempting to access must be addressed using the specified endpoint.");
                    }
                }
                catch (AmazonS3Exception e)
                {
                    return (false, e.Message);
                }
                catch (AggregateException ex)
                {
                    return (false, ex.InnerException.Message);
                }
            }

            return (true, "");
        }

        /// <summary>
        /// 上传文件到AWS的Amazon S3中
        /// </summary>
        /// <param name="S3RegionEndpoint">地区</param>
        /// <param name="S3Accesskey">Account的Access key</param>
        /// <param name="S3Secretkey">Account的Secretkey</param>
        /// <param name="toUploadedFileFullPath">需上传文件的全路径</param>
        /// <param name="S3Bucket">Bucket</param>
        /// <param name="jobLog">日志log</param>
        /// <returns></returns>
        public static async Task UploadFileToAWSAmazonS3(string S3RegionEndpoint, string S3Accesskey, 
            string S3Secretkey, string toUploadedFileFullPath, string S3Bucket, ScheduledJobLog jobLog)
        {

            if (!string.IsNullOrEmpty(S3RegionEndpoint) && !string.IsNullOrEmpty(S3Accesskey)
                && !string.IsNullOrEmpty(S3Secretkey))
            {
                jobLog.AppendDebugInfo($"User S3 start.");
                try
                {
                    var fieldInfo = typeof(RegionEndpoint).GetField(S3RegionEndpoint);
                    var region = (fieldInfo.GetValue(typeof(RegionEndpoint)) as RegionEndpoint);
                    var client = new AmazonS3Client(S3Accesskey, S3Secretkey, region);

                    var folder = string.Empty;
                    var bucketArray = S3Bucket.Split('/');
                    var bucket = bucketArray[0];
                    if (bucketArray.Length > 1)
                    {
                        for (int i = 0; i < bucketArray.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            folder += bucketArray[i] + "/";
                        }
                    }

                    var fielName = Path.GetFileName(toUploadedFileFullPath);
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucket,
                        Key = folder + fielName,
                        FilePath = toUploadedFileFullPath
                    };

                    PutObjectResponse response = await client.PutObjectAsync(putRequest);
                }
                catch (Exception ex)
                {
                    jobLog.AppendDebugInfo($"User S3 exception msg:" + ex.Message);
                    jobLog.AppendDebugInfo($"User S3 exception stackTrace:" + ex.StackTrace);
                }

                jobLog.AppendDebugInfo($"User S3 end.");
            }
        }


    }
}
