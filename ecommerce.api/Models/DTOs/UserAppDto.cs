namespace ecommerce.api.Models.DTOs
{
    public class UserAppDto
    {
        public string Name { get; set; }
        public string JWT { get; set; }
        public string MfaToken { get; set; }
    }
}
