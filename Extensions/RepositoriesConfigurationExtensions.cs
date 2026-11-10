using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Extensions
{
    public static class RepositoriesConfigurationExtensions
    {
        
        public static void AddRepositoriesConfiguration(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();           
        }

    }
}