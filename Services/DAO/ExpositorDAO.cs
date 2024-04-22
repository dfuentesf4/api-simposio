using Npgsql;
using simposio.Models;
using simposio.Services.BDConecction;

namespace simposio.Services.DAO
{
    public class ExpositorDAO
    {
        private readonly DatabaseConnectionService _connectionService;

        public ExpositorDAO(DatabaseConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<List<Expositor>> GetAllAsync()
        {
            List<Expositor> expositores = new List<Expositor>();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "SELECT * FROM expositores";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Expositor expositor = new Expositor
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Tema = reader.GetString(2),
                                Imagen = reader.GetString(3)
                            };

                            expositores.Add(expositor);
                        }

                    }
                    comand.Dispose();
                }
                bdConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return expositores;
        }
    }
}
