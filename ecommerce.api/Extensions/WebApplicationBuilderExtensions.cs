using Microsoft.EntityFrameworkCore;

namespace ecommerce.api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<Data.Context>(options => options.UseSqlServer(connectionString));




            return builder;
        }
    }
}
