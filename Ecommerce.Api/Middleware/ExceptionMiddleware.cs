using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace Ecommerce.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _request;

        public ExceptionMiddleware(RequestDelegate request)
        {
            _request = request;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Ejecuta el siguiente middleware en controlador
                await _request(context);
            }

            catch (Exception ex)
            {
                await ManejarExceptionAsync(context, ex);
            }
        }

        private Task ManejarExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string mensaje;

            // Determinar el código de respuesta según el tipo de excepción
            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;             // 400
                    mensaje = exception.Message;
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;             // 400
                    mensaje = exception.Message;
                    break;

                case SecurityTokenException securityTokenException:
                    statusCode = HttpStatusCode.BadRequest;             // 400
                    mensaje = $"Error al generar token: '{securityTokenException}'";
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;           // 401
                    mensaje = exception.Message;
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;               // 404
                    mensaje = exception.Message;
                    break;

                case DbUpdateException dbEx:                            // Captura explicita de EF Core
                    statusCode = HttpStatusCode.BadRequest;
                    mensaje = dbEx.InnerException != null
                        ? dbEx.InnerException.Message                   // Mensaje de la base de datos
                        : dbEx.Message;
                    break;

                case AutoMapperMappingException autoMapperMappingEx:
                    statusCode = HttpStatusCode.InternalServerError;    // 500
                    mensaje = $"Error de mapeo: '{autoMapperMappingEx}'";
                    break;


                default:
                    statusCode = HttpStatusCode.InternalServerError;    // 500
                    mensaje = $"Ocurrió un error inesperado en el servidor: " + exception.Message;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var respuesta = JsonSerializer.Serialize(new
            {
                detail = mensaje,
                type = exception.GetType().Name,
                status = (int)statusCode
            });

            return context.Response.WriteAsync(respuesta);
        }

    }
}
