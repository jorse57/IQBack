namespace IQ_Api.DTOs
{
    public class usuarioRutasDTO
    {
       public int id { get; set; }
        public bool lectura { get; set; }
        public bool escritura { get; set; }
        public int idUsuario { get; set; }

        public string rutaSubmodulo { get; set; }
        public string nombreSubmodulo { get; set; }
        public string moduloPadre { get; set; }
    }
}
