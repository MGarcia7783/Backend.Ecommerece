using AutoMapper;
using Ecommerce.Api.Mapping;
using Ecommerce.Api.Middleware;
using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Cargar .env y variables de entorno
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();


// Leer variables de conexión desde las variable de entorno
var server = Environment.GetEnvironmentVariable("DB_SEVER");
var port = Environment.GetEnvironmentVariable("DB_PORT");
var database = Environment.GetEnvironmentVariable("DB_DATABASE");
var user = Environment.GetEnvironmentVariable("DB_USER");
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
var key = Environment.GetEnvironmentVariable("JWT_KEY");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");


// Validar variables de entorno para la conexión
var variablesFaltantes = new List<string>();
if (string.IsNullOrWhiteSpace(server)) variablesFaltantes.Add("DB_SEVER");
if (string.IsNullOrWhiteSpace(port)) variablesFaltantes.Add("DB_PORT");
if (string.IsNullOrWhiteSpace(database)) variablesFaltantes.Add("DB_DATABASE");
if (string.IsNullOrWhiteSpace(user)) variablesFaltantes.Add("DB_USER");
if (string.IsNullOrWhiteSpace(password)) variablesFaltantes.Add("DB_PASSWORD");

if(variablesFaltantes.Any())
{
    var menssage = $"Faltan variables de entorno de conexión a la base de datos: {string.Join(", ", variablesFaltantes)}";
    Console.Error.WriteLine(menssage);
}


// Contruir la cadena de conexión en formato SQL Server
var connetionString =
    $"Server={server},{port};" +
    $"Database={database};" +
    $"User Id={user};" +
    $"Password={password};" +
    $"Encrypt=True;" +
    $"TrustServerCertificate=True;";


// Registrar EcommerceDbContext
builder.Services.AddDbContext<EcommerceDbContext>(options => options.UseSqlServer(connetionString));


// Definier las reglas de seguridad para la password y email
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<EcommerceDbContext>()
    .AddDefaultTokenProviders();


// Registrar repositorios con sus interfaces
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IChatBotRepository, ChatBotRepository>();

// Registrar servicios con sus interfaces
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IPagoService, PagoService>();


// Validar la clave secreta
if(string.IsNullOrEmpty(key))
{
    throw new InvalidOperationException("La clave JWT no está configurada correctamente.");
}


// Configurar la autenticación
builder.Services.AddAuthentication
    (
        options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(options =>
    {
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            RoleClaimType = ClaimTypes.Role,
            ValidIssuer = issuer,
            ValidAudience = audience
        };
    });


// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingsProfile));


// Agregar controladores
builder.Services.AddControllers();


// Agregar HttpContextAccessor
builder.Services.AddHttpContextAccessor();


builder.Services.AddOpenApi();


// Construir la aplicacion
var app = builder.Build();


// Comprobar la conexión a la base de datos
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    { 
        // Crear un scope para obtener el DbContext
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();

        // Intentar conectar
        if(dbContext.Database.CanConnect())
        {
            Console.WriteLine($"Conexión a SQL Server establecia. Base de datos: '{database}', Puerto: '{port}'.");
        }
        else
        {
            Console.WriteLine($"No se puedo establecer la conexión a la base de datos.");
        }
    }
    catch(Exception ex)
    {
        Console.Error.WriteLine($"Error al comprobar la conexión a la base de datos: {ex.Message}");
    }
});


// Registrar Middleware para excepciones globales
app.UseMiddleware<ExceptionMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Usar middleware para HTTPS Redirection y autorization
app.UseHttpsRedirection();


// ARCHIVOS ESTÁTICOS
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "ImagenesProductos")),
    RequestPath = "/ImagenesProductos"
});


// Soporte para la autenticación
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

app.Run();
