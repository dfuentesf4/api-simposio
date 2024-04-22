using Npgsql;
using System.Data;

namespace simposio.Services.BDConecction
{
    public class DatabaseConnectionService
    {
        private readonly PostgreSQLConfiguration _config;
        private NpgsqlConnection _npgsqlConnection;

        public DatabaseConnectionService(PostgreSQLConfiguration config)
        {
            _config = config;
            _npgsqlConnection = new NpgsqlConnection(_config.ConnectionString);
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_config.ConnectionString);
        }

        public void OpenConnection()
        {
            if (_npgsqlConnection.State != ConnectionState.Open)
            {
                _npgsqlConnection.Open();
            }
        }

        public NpgsqlConnection GetConnection()
        {
            return _npgsqlConnection;
        }
    }
}
