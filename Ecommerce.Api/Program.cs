using AutoMapper;
using Ecommerce.Api.Mapping;
using Ecommerce.Api.Middleware;
using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Services;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

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


// Registrar repositorios con sus interfaces
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registrar servicios con sus interfaces
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();


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


app.UseAuthorization();

app.MapControllers();

app.Run();
