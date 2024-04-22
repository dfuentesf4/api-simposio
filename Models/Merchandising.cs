using System.ComponentModel.DataAnnotations;

namespace simposio.Models
{
    public class Merchandising
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Nombre { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public string? Imagen { get; set; }

        public List<string> Opciones { get; set; }
    }
}
