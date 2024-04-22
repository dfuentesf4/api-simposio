using System.ComponentModel.DataAnnotations;

namespace simposio.Models
{
    public class Participante
    {
        
        public int Id { get; set;}

        [Required]
        [StringLength(15)]
        [RegularExpression("\\d{4}-\\d{2}-\\d{3,5}", ErrorMessage = "El carnet no cumple con el formato esperado")]
        public string Carnet { get; set; }

        [Required]
        [StringLength(80)]
        public string Nombres { get; set; }

        [Required]
        [StringLength(80)]
        public string Apellidos { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress(ErrorMessage = "El formato del correo electronico no es valido.")]
        public string Correo { get; set; }

        [Required]
        public DateOnly FechaNacimiento { get; set; }
    }
}
