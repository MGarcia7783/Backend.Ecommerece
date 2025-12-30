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
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

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
builder.Services.AddScoped<IChatBotService, ChatBotService>();


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

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = 401,
                    detail = "No autenticado. El token es inválido o no fue enviado."
                }));
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = 403,
                    detail = "Acceso denegado. No tiene permisos para acceder a este recurso."
                }));
            }
        };
    });


// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingsProfile));


// Agregar controladores
builder.Services.AddControllers();


// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();         // Detectar todos los endpoint de API
builder.Services.AddSwaggerGen( options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Ecommerce API",
        Description = """
        #### **Infraestructura escalable para la gestión de comercio digital.**

        Esta API proporciona un conjunto robusto de herramientas para administrar operaciones comerciales complejas, garantizando seguridad, velocidad y una experiencia de usuario optimizada.

        ---

        #### Módulos del Sistema
        * **Catálogo:** Gestión dinámica de productos y categorías con control de stock en tiempo real.
        * **Ventas:** Administración integral de pedidos y seguimiento del ciclo de vida de compra.
        * **Finanzas:** Procesamiento de pagos mediante pasarelas externas y auditoría de transacciones.
        * **Soporte IA:** Chatbot de asistencia para búsqueda inteligente y recomendaciones personalizadas.

        #### Características Técnicas
        * **Seguridad:** Autenticación de grado industrial mediante **JWT**.
        * **Eficiencia:** Consumo de recursos optimizado con soporte para **paginación y filtrado**.
        * **Integración:** Salidas JSON estandarizadas para una fácil implementación en entornos Web y Mobile.

        ---

        """,

        Contact = new OpenApiContact
        {
            Name = "Mario Garcia (Soporte Técnico)",
            Email = "mrgmairena@gmail.com",
            Url = new Uri("https://github.com/MGarcia7783/Backend.Ecommerece")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Configuración de seguridad para Swagger (JWT)

    // 1. Definir el esquema de seguridad que Swagger usará para UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    // 2. Aplicar el esquema de seguridad a toso los endpoint protegidos de la API
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(referenceId: "Bearer", hostDocument: document),
            new List<string>()
        }
    });
});


// Configuración de CORS (Angulr y React)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if(builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                "http://localhost:4200",    // Angular
                "http://localhost:3000"     // React
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(
            "https://www.miappangular.com", // Angular
            "https://www.miappreact.com"    // React
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
        }
    });
});


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
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Usar middleware para HTTPS Redirection y autorization
app.UseHttpsRedirection();


// CORS
app.UseCors("FrontendPolicy");


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
