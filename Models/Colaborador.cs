namespace simposio.Models
{
    public class Colaborador
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
        public Rol Rol { get; set; }
    }

    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
