using System.ComponentModel.DataAnnotations;

namespace ecommerce.api.Models.DTOs.MyProfile
{
    public class EditProfileBaseDto
    {
        [Required]
        public string CurrentPassword { get; set; }

    }
}
