using ecommerce.api.Models.Services.IServices;

namespace ecommerce.api.Models.Services
{
    public class ServiceUnitOfWork : IServiceUnitOfWork
    {
        private readonly IConfiguration configuration;
        public ServiceUnitOfWork(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEmailService EmailService => new EmailService(configuration);

        public ITokenService TokenService => new TokenService(configuration);
    }
}
