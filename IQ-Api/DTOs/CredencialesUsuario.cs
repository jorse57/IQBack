using System.ComponentModel.DataAnnotations;

namespace IQ_Api.DTOs
{
    public class CredencialesUsuario
    {
        public int Id { get; set; }
        public int tipoDocumento { get; set; }
        
        public string? Documento { get; set; }

  
        public string? nombre { get; set; }

        public string? apellido { get; set; }

        public string Email { get; set; }

    
        public string Password { get; set;}

        //public int? rol { get; set; }

    }
}
