using Microsoft.OpenApi;

namespace ApiEcommerce.Extensions
{
    public static class SwaggerConfigExtensions
    {
        
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {

            services.AddSwaggerGen(options =>
            {                             
                //Configuración para que swagger nos permita usar Token en las peticiones.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                                "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                                "Ejemplo: \"12345abcdef\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
                });

                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "API Ecommerce",
                    Description = "API to manage products an users",
                    TermsOfService = new Uri("http://example.com/terms"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Douglas",
                        Url = new Uri("https://douglas.com/")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "Licencia de Uso",
                        Url = new Uri("https://example.com/license")
                    }                  
                });


                options.SwaggerDoc("v2", new OpenApiInfo()
                {
                    Version = "v2",
                    Title = "API Ecommerce V2",
                    Description = "API to manage products an users",
                    TermsOfService = new Uri("http://example.com/terms"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Douglas",
                        Url = new Uri("https://douglas.com/")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "Licencia de Uso",
                        Url = new Uri("https://example.com/license")
                    }                  
                });

            });
        }

    
    }
}