using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Extensions;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Añade la configuración para usar el ORM EFCore de .NET. Por tanto las migraciones, etc.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDataBaseConfiguration(dbConnectionString);


//Añadir configuración para CACHÉ Middleware.
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024;
    options.UseCaseSensitivePaths = true;
});


//Añadir los scopes o inyección de dependencias para los repositorios.
builder.Services.AddRepositoriesConfiguration();

//Maps, para el mapeo de DTOs a entidades (viceversa)
TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);


//Configuration Identity Core to add security to users
builder.Services.AddIdentityCoreConfiguration();

//Configuration to use authorization in API.
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey") ?? throw new InvalidOperationException("Secret Key is not configured");

builder.Services.AddAuthentication(options =>
{   
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; //Dice que no necesita https, por ahora es solo para desarrollo, en producción sí o sí debe ser true.
    options.SaveToken = true; //Guarda el token en el contexto de autenticación.

    options.TokenValidationParameters = new TokenValidationParameters() //crea unos parámetros de configuración.
    {
      ValidateIssuerSigningKey = true, //Se debe validar el token y que esté firmado con una clave válida.
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), //se coloca la clave secreta que se usa para validar el token, la misma que cuando se crea.
      ValidateIssuer = false, //no se va a validar el emisor del token (issuer) 
      ValidateAudience = false //no se va a validar el público del token, útil si no se necesita restringir a ciertos clientes.
    };
});

//Configuration to add versioning API
builder.Services.AddVersioningConfiguration();

//Add API Documentation
builder.Services.AddSwaggerConfiguration();

builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add(CacheProfiles.DEFAULT_10, CacheProfiles.Profile10);
    options.CacheProfiles.Add(CacheProfiles.DEFAULT_2O, CacheProfiles.Profile20);
}
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//Set CORS (Cross-Origin Resources Sharing) to allow any origin or specifics origins.

builder.Services.AddCors(options =>
{
    
    options.AddPolicy(PolicyNames.ALLOW_SPECIFIC_ORIGIN, 
    builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    });

});

// ************************
var app = builder.Build();
// ************************

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{   
    app.UseSwagger(options =>
    {
        /*
             Cambias la ruta por defecto del JSON generado
             pasa de ser /swagger/v1/swagger.json a api-docs/v1/json
        */
       // options.RouteTemplate = "api-docs/{documentName}/json";
    });


    app.UseSwaggerUI(options =>
    {
        /*
            Línea de configuración para indicarle explicitamente donde buscar el archivo json para
            encontrar la documentación de los endpoints.
        */      
        //options.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiEcommerce v1");

        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
         options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
    app.MapOpenApi();
}

app.UseStaticFiles(); //Permite el uso de archivos imagenes, css, pdf etc.

app.UseHttpsRedirection();

app.UseCors(PolicyNames.ALLOW_SPECIFIC_ORIGIN);

app.UseResponseCaching(); //Siempre debe ir después del CORS.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
