using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Hms.Repository
{
    public interface IBaseRepository
    {
        void SetIdentity<T>(IDbConnection connection, Action<T> setId);
        IDbConnection CreateConnection();
        Task SetConnectionString(string resolvedConnectionString);
    }

    public class BaseRepository : IBaseRepository
    {
        private string connectionString;

        public BaseRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("HmsConnectionString")
        ?? throw new InvalidOperationException("HmsConnectionString not found in configuration");
        }

        public async Task SetConnectionString(string resolvedConnectionString)
        {
            await Task.Run(() =>
            {
                connectionString = resolvedConnectionString;
            });
        }
        
        public void SetIdentity<T>(IDbConnection connection, Action<T> setId)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open");

            dynamic identity = connection.Query("SELECT @@IDENTITY AS Id").Single();
            T newId = (T)identity.Id;
            setId(newId);
        }

        public virtual IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}