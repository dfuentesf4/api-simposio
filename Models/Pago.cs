namespace simposio.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public Participante Participante { get; set; }
        public decimal Monto { get; set; }
        public bool Verificado { get; set; }
        public string ImagenBoleta { get; set;}
        public DateTime FechaHora {  get; set; }
    }
}
