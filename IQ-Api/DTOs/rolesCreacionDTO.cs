using System.ComponentModel.DataAnnotations;

namespace IQ_Api.DTOs
{
    public class rolesCreacionDTO
    {
        [Required]
        public string? rolName { get; set; }

        public string? rolDescription { get; set; }
    }
}
