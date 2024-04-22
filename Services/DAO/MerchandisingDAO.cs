using Npgsql;
using simposio.Models;
using simposio.Services.BDConecction;

namespace simposio.Services.DAO
{
    public class MerchandisingDAO
    {
        private readonly DatabaseConnectionService _connectionService;

        public MerchandisingDAO(DatabaseConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<List<Merchandising>> GetAllAsync()
        {
            List<Merchandising> merchindisings = new List<Merchandising>();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "SELECT * FROM merchandising";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Merchandising merchindising = new Merchandising
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Imagen = reader.GetString(3),
                                Opciones = new()
                            };

                            merchindisings.Add(merchindising);
                        }

                    }
                    comand.Dispose();
                }

                string opcionesQuery = "SELECT Nombre, Producto FROM opciones WHERE Producto = ANY(@Ids)";
                var merchIds = merchindisings.Select(m => m.Id).ToArray();

                using (var opcionesCommand = new NpgsqlCommand(opcionesQuery, bdConnection))
                {
                    opcionesCommand.Parameters.AddWithValue("Ids", merchIds);
                    using (var opcionesReader = await opcionesCommand.ExecuteReaderAsync())
                    {
                        while (await opcionesReader.ReadAsync())
                        {
                            var nombreOpcion = opcionesReader.GetString(opcionesReader.GetOrdinal("Nombre"));
                            var productoId = opcionesReader.GetInt32(opcionesReader.GetOrdinal("Producto"));
                            var merch = merchindisings.FirstOrDefault(m => m.Id == productoId);
                            merch?.Opciones.Add(nombreOpcion);
                        }
                    }
                }

                await bdConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return merchindisings;
        }
    }
}
