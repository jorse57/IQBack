using System.ComponentModel.DataAnnotations;

namespace IQ_Api.Entidades
{
    public class Roles
    {
        public int Id { get; set; }
       
        [Required]
        public string? rolName { get; set; }
        
        public string? rolDescription { get; set; }
    }
}
