using Dapper;
using Hms.Domains;
using Hms.Domains.User;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Hms.Repository
{
    #region Interface
    public interface IUserRepository
    {
        Task<GenericResult> Register(Domains.User.User userRequest, int userId, int tenantId);
        Task<UserGet> Validate(string email, string password);
        Task<int> UpdateUserAccessCode(string accessCode, int loggedinUserId);
        Task<int> ValidateAccessCode(int userId, int timeValid, string accessCode);
        Task<UserGet> GetByUserId(int userId, int tenantId);
    }
    #endregion

    public class UserRepository : BaseRepository, IUserRepository
    {
        #region Properties
        private readonly IConfiguration configuration;
        #endregion

        #region Constructor
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
            this.configuration = configuration;
        }
        #endregion

        #region Register
        public async Task<GenericResult> Register(Domains.User.User userRequest, int userId, int tenantId)
        {
            var userSave = new GenericResult();
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameter = new DynamicParameters();
                dynamicParameter.Add("@TenantId", tenantId);
                dynamicParameter.Add("@FirstName",userRequest.FirstName);
                dynamicParameter.Add("@LastName",userRequest.LastName);
                dynamicParameter.Add("@Email", userRequest.Email);
                dynamicParameter.Add("@Password",userRequest.Password);
                dynamicParameter.Add("@AgreeToTerms",userRequest.AgreeToTerms);
                dynamicParameter.Add("@LoggedInUserId", userId);
                dynamicParameter.Add("@RoleId", userRequest.RoleId);

                dynamicParameter.Add("@NewId", DbType.Int32, direction: ParameterDirection.Output);
                dynamicParameter.Add("@ReturnValue", DbType.Int32, direction: ParameterDirection.ReturnValue);
                dbConnection.Open();

                await dbConnection.ExecuteAsync("[dbo].[User_Register]", dynamicParameter, commandType: CommandType.StoredProcedure);
                userSave.NewId = dynamicParameter.Get<int?>("@NewId");
                userSave.ReturnValue = dynamicParameter.Get<int?>("@ReturnValue");
            }
            return userSave;
        }
        #endregion

        #region Validate
        public async Task<UserGet> Validate(string email, string password)
        {
            var response = new UserGet();
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@Email", email);
                dynamicParameters.Add("@Password", password);
                dynamicParameters.Add(name: "@ReturnVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                dbConnection.Open();

                var result = await dbConnection.QueryMultipleAsync("[dbo].[User_Validate]", dynamicParameters, commandType: CommandType.StoredProcedure);

                var returnValue = dynamicParameters.Get<int?>("@ReturnVal");

                if (returnValue == 0)
                {
                    response.User = result.Read<User>().FirstOrDefault();
                    response.ReturnValue = dynamicParameters.Get<int>("@ReturnVal");
                }
                else
                {
                    response.ReturnValue = dynamicParameters.Get<int>("@ReturnVal");
                }

                return response;
            }
        }
        #endregion

        #region UpdateUserAccessCode
        public async Task<int> UpdateUserAccessCode(string accessCode, int loggedinUserId)
        {
            int retValue = -1;
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameter = new DynamicParameters();
                dynamicParameter.Add("@UserId", loggedinUserId);
                dynamicParameter.Add("@AccessCode", accessCode);
                dynamicParameter.Add(name: "@ReturnVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                await dbConnection.ExecuteAsync("[dbo].[User_UpdateAccessCode]", dynamicParameter, commandType: CommandType.StoredProcedure);
                retValue = dynamicParameter.Get<int>("@ReturnVal");

                return retValue;
            }
        }
        #endregion

        #region ValidateAccessCode
        public async Task<int> ValidateAccessCode(int userId, int timeValid, string accessCode)
        {
            int returnValue;
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@UserId", userId);
                dynamicParameters.Add("@TimeValid", timeValid);
                dynamicParameters.Add("@AccessCode", accessCode);
                dynamicParameters.Add(name: "@ReturnVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                dbConnection.Open();
                await dbConnection.ExecuteAsync("[dbo].[User_ValidateAccessCode]", dynamicParameters, commandType: CommandType.StoredProcedure);
                returnValue = dynamicParameters.Get<int>("@ReturnVal");

                return returnValue;
            }
        }
        #endregion

        #region GetByUserId
        public async Task<UserGet> GetByUserId(int userId, int tenantId)
        {
            var response = new UserGet();
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameter = new DynamicParameters();
                dynamicParameter.Add("@UserId", userId);
                dynamicParameter.Add("@TenantId", tenantId);
                dynamicParameter.Add(name: "@ReturnVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                dbConnection.Open();
                var result = await dbConnection.QueryMultipleAsync("[dbo].[User_GetById]", dynamicParameter, null, null, commandType: CommandType.StoredProcedure);
                response.User = result.Read<User?>().FirstOrDefault();

                response.ReturnValue = dynamicParameter.Get<int?>("@ReturnVal");
                return response;
            }
        }
        #endregion
    }
}
