namespace simposio.Services.BDConecction
{
    public class PostgreSQLConfiguration
    {
        public string ConnectionString { get; set; }

        public PostgreSQLConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
