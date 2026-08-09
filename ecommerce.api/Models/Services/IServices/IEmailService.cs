using ecommerce.api.Models.DTOs;

namespace ecommerce.api.Models.Services.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailSendDto emailSend);
    }
}
