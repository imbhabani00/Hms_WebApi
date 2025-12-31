using AutoMapper;
using Hms.Domains;
using Hms.Domains.User;
using Hms.Helpers;
using Hms.Infrastructure.AWS;
using Hms.Repository;
using Hms.Service.Request.Users;
using Hms.Service.Response;
using Hms.Service.Response.Users;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Hms.Service
{
    #region Interface
    public interface IUserService
    {
        Task<GenericResponse> RegisterAsync(UserRequest userRequest, int userId, int tenantId);
        Task<UserResponseGet> Validate(string email, string password);
        Task<int> UpdateUserAccessCode(int loggedInUserId, int tenantId);
        Task<int> ValidateAccessCode(int userId, int timeValid, string accessCode);
        Task<UserResponseGet> GetByUserId(int userId, int tenantId);
    }
    #endregion
    public class UserService : BaseService, IUserService
    {
        #region Properties
        private IAWSS3Service _aWSS3Service;
        private readonly AccessCodeHelper _accessCodeHelper;
        private IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IEmailSenderService _emailViewRenderer;
        #endregion

        #region Constructor
        public UserService(
            IMapper mapper,
            IAWSS3Service aWSS3Service,
            IConfiguration configuration,
            AccessCodeHelper accessCodeHelper,
            IUserRepository userRepository,
            IEmailSenderService emailViewRenderer,
            IEmailService emailService
            ) : base(mapper, configuration)
        {
            _aWSS3Service = aWSS3Service;
            _accessCodeHelper = accessCodeHelper;
            _userRepository = userRepository;
            _emailService = emailService;
            _emailViewRenderer = emailViewRenderer;
        }
        #endregion

        #region Register
        public async Task<GenericResponse> RegisterAsync(UserRequest userRequest, int userId, int tenantId)
        {
            var genericResponse = new GenericResponse();
            try
            {
                //Encrypt Password
                string bsmKey = _configuration.GetValue<string>("BSMSecret");
                string encryptedPassword = BSMEncryptDecryptHelper.Encrypt(userRequest.Password, bsmKey, 256);

                var user = _mapper.Map<UserRequest, Domains.User.User>(userRequest);
                user.Password = encryptedPassword;

                var saveResult = await _userRepository.Register(user, userId, tenantId);

                if (saveResult.ReturnValue == 0 && saveResult.NewId > 0)
                {
                    await _emailService.SendRegistrationEmail(userRequest, string.Empty);

                    genericResponse = _mapper.Map<GenericResult, GenericResponse>(saveResult);
                    genericResponse.Message = "Registration successful! Please check your email.";

                    Log.Information("User registered successfully: {Email}, UserId: {UserId}",
                        userRequest.Email, saveResult.NewId);
                }
                else
                {
                    genericResponse.ReturnValue = saveResult.ReturnValue ?? -1;
                    genericResponse.Message = "Registration failed";

                    Log.Warning("User registration failed: {Email}, ReturnValue: {ReturnValue}",
                        userRequest.Email, saveResult.ReturnValue);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RegisterAsync for email: {Email}", userRequest.Email);
                genericResponse.ReturnValue = -1;
                genericResponse.Message = "An error occurred during registration";
            }

            return genericResponse;
        }
        #endregion

        #region Validate
        public async Task<UserResponseGet> Validate(string email, string password)
        {
            var request = await _userRepository.Validate(email, password);
            var response = _mapper.Map<UserGet, UserResponseGet>(request);
            return response;
        }
        #endregion

        #region UpdateUserAccessCode
        public async Task<int> UpdateUserAccessCode(int loggedInUserId, int tenantId)
        {
            int returnValue = -1;
            try
            {
                var accessCode = _accessCodeHelper.GenerateSecurityCode();
                returnValue = await _userRepository.UpdateUserAccessCode(accessCode.ToString(), loggedInUserId);

                if (returnValue == 0)
                {
                    var user = await _userRepository.GetByUserId(loggedInUserId, tenantId);

                    if (user?.User != null)
                    {
                        await _emailService.SendAccessCodeEmail(user.User, accessCode.ToString());
                        Log.Information("Access code sent to: {Email}", user.User.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating access code for UserId: {UserId}", loggedInUserId);
            }

            return returnValue;
        }
        #endregion

        #region ValidateAccessCode
        public async Task<int> ValidateAccessCode(int userId, int timeValid, string accessCode)
        {
            int returnValue = -1;
            try
            {
                returnValue = await _userRepository.ValidateAccessCode(userId, timeValid, accessCode);

                if (returnValue == 0)
                {
                    Log.Information("Access code validated for UserId: {UserId}", userId);
                }
                else
                {
                    Log.Warning("Invalid access code for UserId: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating access code for UserId: {UserId}", userId);
            }

            return returnValue;
        }
        #endregion

        #region GetByUserId
        public async Task<UserResponseGet> GetByUserId(int userId, int tenantId)
        {
            var request = await _userRepository.GetByUserId(userId, tenantId);
            var response = _mapper.Map<UserGet, UserResponseGet>(request);
            return response;
        }
        #endregion
    }
}

