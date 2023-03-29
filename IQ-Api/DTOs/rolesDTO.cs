using System.ComponentModel.DataAnnotations;

namespace IQ_Api.DTOs
{
    public class rolesDTO
    {
        public int Id { get; set; }

        [Required]
        public string? rolName { get; set; }

        public string? rolDescription { get; set; }
    }
}
