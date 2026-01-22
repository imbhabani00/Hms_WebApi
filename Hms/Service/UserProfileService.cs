using Amazon.Runtime.Internal.Util;
using AutoMapper;
using Hms.Domains.UserProfile;
using Hms.Helpers;
using Hms.Infrastructure.AWS;
using Hms.Repository;
using Hms.Service.Response.UserProfile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace Hms.Service
{
    public interface IUserProfileService
    {
        Task<UserProfileResponseGet> GetUserProfileAsync(int userId, int tenantId);
    }
    public class UserProfileService : BaseService, IUserProfileService
    {


        #region Properties
        private IAWSS3Service _aWSS3Service;
        private readonly AccessCodeHelper _accessCodeHelper;
        private IUserProfileRepository _userProfileRepository;
        private readonly IEmailService _emailService;
        private readonly IEmailSenderService _emailViewRenderer;
        private readonly ILogger<UserProfileService> _logger;

        #endregion

        #region Constructor
        public UserProfileService(
            IMapper mapper,
            IAWSS3Service aWSS3Service,
            IConfiguration configuration,
            AccessCodeHelper accessCodeHelper,
            IUserProfileRepository userProfileRepository,
            IEmailSenderService emailViewRenderer,
            IEmailService emailService,
            ILogger<UserProfileService> logger
            ) : base(mapper, configuration)
        {
            _aWSS3Service = aWSS3Service;
            _accessCodeHelper = accessCodeHelper;
            _userProfileRepository = userProfileRepository;
            _emailService = emailService;
            _emailViewRenderer = emailViewRenderer;
            _logger = logger;
        }
        #endregion

        public async Task<UserProfileResponseGet> GetUserProfileAsync(int userId, int tenantId)
        {
            try
            {
                var userProfile = await _userProfileRepository.GetUserProfile(userId, tenantId);

                if (userProfile == null)
                {
                    _logger.LogWarning("User profile not found for UserId: {UserId}", userId);
                    return null;
                }

                if (!string.IsNullOrEmpty(userProfile.User.ProfilePicturePath))
                {
                    if (_aWSS3Service != null)
                    {
                        try
                        {
                            userProfile.User.ProfilePicturePath = await _aWSS3Service.GetDocumentUrl(userProfile.User.ProfilePicturePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to get S3 URL, using local path");
                            var baseUrl = _configuration.GetValue<string>("HmsApiUrl");
                            userProfile.User.ProfilePicturePath = $"{baseUrl}/uploads/{userProfile.User.ProfilePicturePath}";
                        }
                    }
                    else
                    {
                        // Use local storage
                        var baseUrl = _configuration.GetValue<string>("HmsApiUrl");
                        userProfile.User.ProfilePicturePath = $"{baseUrl}/uploads/{userProfile.User.ProfilePicturePath}";
                    }
                }

                var response = _mapper.Map<UserProfileGet, UserProfileResponseGet>(userProfile);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for UserId: {UserId}", userId);
                throw;
            }
        }
    }
}
