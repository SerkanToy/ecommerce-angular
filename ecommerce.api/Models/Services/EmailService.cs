 using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.Services.IServices;
using System.Net;
using System.Net.Mail;

namespace ecommerce.api.Models.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<bool> SendEmailAsync(EmailSendDto emailSend)
        {
            try
            {
                var username = configuration["EmailSettings:Username"];
                var password = configuration["EmailSettings:Password"];

                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                (
                    from: username, to: emailSend.To, subject: emailSend.Subject, body: emailSend.Body
                )
                { 
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
