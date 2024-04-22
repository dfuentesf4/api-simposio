using Npgsql;
using simposio.Models;
using simposio.Services.BDConecction;

namespace simposio.Services.DAO
{
    public class ParticipanteDAO
    {
        private readonly DatabaseConnectionService _connectionService;

        public ParticipanteDAO(DatabaseConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<List<Participante>> GetAllAsync()
        {
            List<Participante> participantes = new List<Participante>();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "SELECT * FROM participantes";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Participante participante = new Participante
                            {
                                Id = reader.GetInt32(0),
                                Carnet = reader.GetString(1),
                                Nombres = reader.GetString(2),
                                Apellidos = reader.GetString(3),
                                Correo = reader.GetString(4),
                                FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime(5))
                            };

                            participantes.Add(participante);
                        }

                    }
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return participantes;
        }

        public async Task<Participante> GetByCarnetAsync(string carnet)
        {
            Participante participante = null;
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "SELECT * FROM participantes WHERE carnet = @carnet";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@carnet", carnet);

                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            participante = new Participante
                            {
                                Id = reader.GetInt32(0),
                                Carnet = reader.GetString(1),
                                Nombres = reader.GetString(2),
                                Apellidos = reader.GetString(3),
                                Correo = reader.GetString(4),
                                FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime(5))
                            };
                        }
                    }
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return participante;
        }

        public List<Participante> GetAssisted()
        {
            List<Participante> participantes = new List<Participante>();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                bdConnection.Open();

                string query = @"SELECT * 
                                FROM participantes p
                                INNER JOIN public.control c ON p.id = c.participanteid
                                WHERE c.asistencia = true";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    using (var reader = comand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Participante participante = new Participante
                            {
                                Id = reader.GetInt32(0),
                                Carnet = reader.GetString(1),
                                Nombres = reader.GetString(2),
                                Apellidos = reader.GetString(3),
                                Correo = reader.GetString(4),
                                FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime(5))
                            };

                            participantes.Add(participante);
                        }

                    }
                    comand.Dispose();
                    bdConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return participantes;
        }

        public async Task<bool> InsertAsync(Participante participante)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "INSERT INTO participantes (carnet, nombres, apellidos, correo, fechanacimiento) VALUES (@carnet, @nombres, @apellidos, @correo, @fecha_nacimiento)";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@carnet", participante.Carnet);
                    comand.Parameters.AddWithValue("@nombres", participante.Nombres);
                    comand.Parameters.AddWithValue("@apellidos", participante.Apellidos);
                    comand.Parameters.AddWithValue("@correo", participante.Correo);
                    comand.Parameters.AddWithValue("@fecha_nacimiento", participante.FechaNacimiento);

                    await comand.ExecuteNonQueryAsync();
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> AddMerchandisingAsync(int idParticipante, int idMerchandising, int cantidad, string opcion)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "INSERT INTO participantes_merchandising (participanteid, producto, cantidad, opcion) VALUES (@id_participante, @id_merchandising, @cantidad, @opcion)";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@id_participante", idParticipante);
                    comand.Parameters.AddWithValue("@id_merchandising", idMerchandising);
                    comand.Parameters.AddWithValue("@cantidad", cantidad);
                    comand.Parameters.AddWithValue("@opcion", opcion == null? DBNull.Value : opcion.ToLower());

                    await comand.ExecuteNonQueryAsync();
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> AddDetallePagoAsync(int participanteId, decimal Monto)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "INSERT INTO detalle_pago (participanteid, monto) VALUES (@participanteid, @monto)";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@participanteid", participanteId);
                    comand.Parameters.AddWithValue("@monto", Monto);

                    await comand.ExecuteNonQueryAsync();
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<int> GetDetallePagoByCarnetAsync(string carnet)
        {
            int id = 0;
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "SELECT * FROM detalle_pago WHERE participanteid = (SELECT id FROM participantes WHERE carnet = @carnet)";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@carnet", carnet);

                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            id = reader.GetInt32(0);
                            return id;
                        }
                    }
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return id;
        }

        public async Task<bool> AddPagoAsync(int detallePagoId, string imagen, DateTime fechaHora)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "INSERT INTO verificacion_pago (detallepagoid, verificado, imagen, fechahora) VALUES (@detallepagoid, @verificado, @imagen, @fechahora)";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@detallepagoid", detallePagoId);
                    comand.Parameters.AddWithValue("@verificado", false);
                    comand.Parameters.AddWithValue("@imagen", imagen);
                    comand.Parameters.AddWithValue("@fechahora", fechaHora);

                    await comand.ExecuteNonQueryAsync();
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> PagoVerificadoAsync(int pagoId)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = "UPDATE verificacion_pago SET verificado = true WHERE id = @id";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@id", pagoId);

                    await comand.ExecuteNonQueryAsync();
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<List<Pago>> GetPagosAsync()
        {
            List<Pago> pagos = new List<Pago>();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = @"SELECT * FROM Participantes p 
                                INNER JOIN Detalle_pago dp ON p.id = dp.participanteid
                                INNER JOIN verificacion_pago vp ON dp.id = vp.detallepagoid";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Pago pago = new Pago
                            {
                                Id = reader.GetInt32(9),
                                Participante = new Participante
                                {
                                    Id = reader.GetInt32(0),
                                    Carnet = reader.GetString(1),
                                    Nombres = reader.GetString(2),
                                    Apellidos = reader.GetString(3),
                                    Correo = reader.GetString(4),
                                    FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime(5))
                                },
                                Monto = reader.GetDecimal(8),
                                Verificado = reader.GetBoolean(11),
                                ImagenBoleta = reader.GetString(12),
                                FechaHora = reader.GetDateTime(13)
                            };

                            pagos.Add(pago);
                        }

                    }
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return pagos;
        }

        public async Task<DetallePago> GetDetallePagoCorreoAsync(int participanteId)
        {
            DetallePago dp = new();
            dp.Merchandisings = new();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                await bdConnection.OpenAsync();

                string query = @"SELECT * FROM participantes p
                                INNER JOIN participantes_merchandising pm ON p.id = pm.participanteid
                                INNER JOIN merchandising m ON pm.producto = m.id
                                INNER JOIN detalle_pago dp ON p.id = dp.participanteid
                                WHERE p.id = @participanteid";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@participanteid", participanteId);

                    using (var reader = await comand.ExecuteReaderAsync())
                    {
                        
                        while (await reader.ReadAsync())
                        {
                            dp.Participante = new Participante
                            {
                                Id = reader.GetInt32(0),
                                Carnet = reader.GetString(1),
                                Nombres = reader.GetString(2),
                                Apellidos = reader.GetString(3),
                                Correo = reader.GetString(4),
                                FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime(5))
                            };

                            participantes_merchandising pm = new()
                            {
                                id = reader.GetInt32(6),
                                participante_id = reader.GetInt32(7),
                                cantidad = reader.GetInt32(9),
                                opcion = reader.IsDBNull(10)? null : reader.GetString(10)
                            };

                            pm.merchandising = new Merchandising
                            {
                                Id = reader.GetInt32(11),
                                Nombre = reader.GetString(12),
                                Precio = reader.GetDecimal(13),
                                Imagen = reader.GetString(14)
                            };

                            dp.Monto = reader.GetDecimal(17);

                            
                            dp.Merchandisings.Add(pm);
                        }

                    }
                    comand.Dispose();
                    bdConnection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dp;
        }
    }
}
