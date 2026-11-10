using Asp.Versioning;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace ApiEcommerce.Extensions
{
    public static class ApiVersioningExtensions
    {
        
        public static IServiceCollection AddVersioningConfiguration(this IServiceCollection services)
        {

            var apiVersioningBuilder = services.AddApiVersioning(option =>
            {
                /*
                    Configuración por si el cliente no especifica el versionamiento, por defecto se le envía el configurado en
                    DefaultApiVersion.
                */
                option.DefaultApiVersion = new ApiVersion(1.0);
                option.AssumeDefaultVersionWhenUnspecified = true;

                /*
                    Envia el api-supported-versions y api-deprecated-versions  en el HEADERS de las respuestas. 
                    Añade ReportApiVersionsAttribute como filtro global en la configuración
                */
                option.ReportApiVersions = true; 

                /*
                    Esto añade en el swagger 
                */
                //option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version")); //?api-version es para usar por medio de querym params.
            
            })
            .AddMvc()           
            .AddApiExplorer(option =>
            {
                option.GroupNameFormat = "'v'VVV"; //v1, v2, v3,...
                option.SubstituteApiVersionInUrl = true; // api/v{version}/products
            });

            return services;
        }

    }
}