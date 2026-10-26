using ecommerce.utility;
using System.ComponentModel.DataAnnotations;

namespace ecommerce.api.Models.DTOs
{
    public class MfaDisableDto
    {
        [Required]
        public string Token { get; set; }
        private string _email;
        [Required]
        [RegularExpression(SD.EmailRegex, ErrorMessage = "Invalid Email Address")]
        public string Email { get { return _email; } set { _email = value; } }
    }
}
