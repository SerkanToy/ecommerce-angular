using ecommerce.utility;
using System.ComponentModel.DataAnnotations;

namespace ecommerce.api.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "E-Posta en az 3 en fazla 50 karakter olmalıdır.")]
        [RegularExpression(SD.EmailRegex, ErrorMessage = "Geçerli E-Posta Adresi giriniz.")]
        public string Email { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad en az 2 en fazla 50 karakter olmalıdır.")]
        [RegularExpression(SD.NameRegex, ErrorMessage = "Ad sadece harflerden oluşmalıdır.")]
        public string LastName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad en az 2 en fazla 50 karakter olmalıdır.")]
        [RegularExpression(SD.NameRegex, ErrorMessage = "Soyad sadece harflerden oluşmalıdır.")]
        public string FisrtName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Şifre en az 6 en fazla 50 karakter olmalıdır.")]
        public string Password { get; set; }
    }
}
