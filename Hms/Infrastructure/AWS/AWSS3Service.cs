using Amazon.S3;
using Amazon.S3.Model;
using Hms.Extension;
using Hms.WebApi.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Hms.Infrastructure.AWS
{
    #region Interface
    public interface IAWSS3Service
    {
        Task<bool> UploadFileAsync(string filePath, string bucketPath, bool isPublic = false);
        Task<string> GetDocumentUrl(string filePath, bool isPublic = false);
    }
    #endregion

    public class AWSS3Service : IAWSS3Service
    {
        #region Properties
        private readonly IAmazonS3 _s3Client;
        private readonly S3Options _privateOptions;
        private readonly S3PublicOptions _publicOptions;
        private readonly ILogger<AWSS3Service> _logger;
        #endregion

        #region Constructor
        public AWSS3Service(
            IAmazonS3 s3Client,
            IOptions<S3Options> privateOptions,
            IOptions<S3PublicOptions> publicOptions,
            ILogger<AWSS3Service> logger
            )
        {
            _s3Client = s3Client;
            _privateOptions = privateOptions.Value;
            _publicOptions = publicOptions.Value;
            _logger = logger;
        }
        #endregion

        #region GetDocumentUrl
        public async Task<string> GetDocumentUrl(string filePath, bool isPublic = false)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = isPublic ? _publicOptions.BucketName : _privateOptions.BucketName,
                Key = filePath,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(_privateOptions.SignedUrlExpiresMinutes)
            };

            return _s3Client.GetPreSignedURL(request);
        }
        #endregion

        #region UploadFileAsync
        public async Task<bool> UploadFileAsync(string filePath, string bucketPath, bool isPublic = false)
        {
            try
            {
                var uniqueFilename = Guid.NewGuid().GenerateUniqueId();
                var finalKey = $"{uniqueFilename}/{bucketPath}";

                var request = new PutObjectRequest
                {
                    BucketName = isPublic ? _publicOptions.BucketName : _privateOptions.BucketName,
                    Key = finalKey,
                    FilePath = filePath,
                    CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.BucketOwnerFullControl
                };

                var response = await _s3Client.PutObjectAsync(request);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3 upload failed for {BucketPath}", bucketPath);
                return false;
            }
        }
        #endregion
    }
}