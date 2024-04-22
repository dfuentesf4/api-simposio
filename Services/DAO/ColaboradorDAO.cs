using Npgsql;
using simposio.Models;
using simposio.Services.BDConecction;

namespace simposio.Services.DAO
{
    public class ColaboradorDAO
    {

        private readonly DatabaseConnectionService _connectionService;

        public ColaboradorDAO(DatabaseConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public bool Insert(Colaborador colaborador)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                bdConnection.Open();

                string query = "INSERT INTO colaboradores (usuario, contraseña,rol) VALUES (@usuario, @contraseña, @rol)";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@usuario", colaborador.Usuario);
                    comand.Parameters.AddWithValue("@contraseña", colaborador.Contraseña);
                    comand.Parameters.AddWithValue("@rol", colaborador.Rol.Id);

                    comand.ExecuteNonQuery();
                    comand.Dispose();
                }
                bdConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public Colaborador GetColaborador(string usuario)
        {
            Colaborador colaborador = new Colaborador();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                bdConnection.Open();

                string query = "SELECT * FROM colaboradores c INNER JOIN roles r ON c.rol = r.id WHERE usuario = @usuario";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@usuario", usuario);

                    using (var reader = comand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            colaborador.Id = reader.GetInt32(0);
                            colaborador.Usuario = reader.GetString(1);
                            colaborador.Contraseña = reader.GetString(2);
                            colaborador.Rol = new Rol()
                            {
                                Id = reader.GetInt32(3),
                                Nombre = reader.GetString(5)
                            };
                        }
                    }
                }
                bdConnection.Close();
                return colaborador;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool UpdatePassword(Colaborador colaborador)
        {
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                bdConnection.Open();

                string query = "UPDATE colaboradores SET contraseña = @contraseña WHERE id = @id";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@contraseña", colaborador.Contraseña);
                    comand.Parameters.AddWithValue("@id", colaborador.Id);

                    comand.ExecuteNonQuery();
                    comand.Dispose();
                }
                bdConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public Colaborador requestAccess(Colaborador colaborador)
        {
            bool exists = false;
            Colaborador col = new Colaborador();
            NpgsqlConnection bdConnection = (NpgsqlConnection)_connectionService.CreateConnection();
            try
            {
                bdConnection.Open();

                string query = @"SELECT * FROM colaboradores c
                                    INNER JOIN roles r ON c.rol = r.id
                                    WHERE usuario = @usuario";

                using (var comand = new NpgsqlCommand(query, bdConnection))
                {
                    comand.Parameters.AddWithValue("@usuario", colaborador.Usuario);

                    var reader = comand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        exists = true;
                        reader.Read();
                        col.Id = reader.GetInt32(0);
                        col.Usuario = reader.GetString(1);
                        col.Contraseña = reader.GetString(2);
                        col.Rol = new Rol()
                        {
                            Id = reader.GetInt32(3),
                            Nombre = reader.GetString(5)
                        };
                    }

                    comand.Dispose();
                }

                if (exists)
                {
                    if (BCrypt.Net.BCrypt.Verify(colaborador.Contraseña, col.Contraseña)) ;
                    else col = null;
                }
                bdConnection.Close();

                return col;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                col = null;
                return col;
            }
        }
    }
}
