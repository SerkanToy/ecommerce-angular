using ecommerce.utility;
using System.ComponentModel.DataAnnotations;

namespace ecommerce.api.Models.DTOs
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }

        private string _email;
        [Required]
        [RegularExpression(SD.EmailRegex, ErrorMessage = "Invalid email address")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }
    }

    public class EmailDto
    {
        private string _email;
        [Required]
        [RegularExpression(SD.EmailRegex, ErrorMessage = "Invalid email address")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }
    }
}
