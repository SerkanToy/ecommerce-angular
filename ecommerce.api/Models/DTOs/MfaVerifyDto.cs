using System.ComponentModel.DataAnnotations;

namespace ecommerce.api.Models.DTOs
{
    public class MfaVerifyDto
    {
        [Required]
        public string MfaToken { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
