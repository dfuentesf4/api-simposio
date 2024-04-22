namespace simposio.Models
{
    public class DetallePago
    {
        public List<participantes_merchandising> Merchandisings { get; set; }
        public decimal Monto { get; set; }
        public Participante Participante { get; set; }

    }

    public class participantes_merchandising
    {         
        public int id { get; set; }
        public int participante_id { get; set; }
        public Merchandising merchandising { get; set; }
        public int cantidad { get; set; }
        public string opcion { get; set; }
    }
}


