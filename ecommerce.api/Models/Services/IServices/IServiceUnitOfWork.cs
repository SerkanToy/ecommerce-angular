namespace ecommerce.api.Models.Services.IServices
{
    public interface IServiceUnitOfWork
    {
        IEmailService EmailService { get; }
        ITokenService TokenService { get; }
    }
}

