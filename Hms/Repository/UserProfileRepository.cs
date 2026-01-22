using Dapper;
using Hms.Domains.UserProfile;
using Hms.Service.Request.Users;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Hms.Repository
{
    public interface IUserProfileRepository
    {
        Task<UserProfileGet> GetUserProfile(int userId, int tenantId);
    }
    public class UserProfileRepository : BaseRepository, IUserProfileRepository
    {
        #region Properties
        private readonly IConfiguration configuration;
        #endregion

        #region Constructor
        public UserProfileRepository(IConfiguration configuration) : base(configuration)
        {
            this.configuration = configuration;
        }
        #endregion

        #region GetUserProfile
        public async Task<UserProfileGet> GetUserProfile(int userId, int tenantId)
        {
            var user = new UserProfileGet();
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameter = new DynamicParameters();
                dynamicParameter.Add("@TenantId", tenantId);
                dynamicParameter.Add("@UserId", userId);
                dynamicParameter.Add("@ReturnValue", DbType.Int32, direction: ParameterDirection.ReturnValue);
                dbConnection.Open();

                await dbConnection.QueryMultipleAsync("[dbo].[User_Register]", dynamicParameter, commandType: CommandType.StoredProcedure);
                user.ReturnValue = dynamicParameter.Get<int?>("@ReturnValue");
                dbConnection.Close();
            }
            return user;
        }
        #endregion
    }
}
