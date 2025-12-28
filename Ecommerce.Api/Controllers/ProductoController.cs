using Ecommerce.Api.Request;
using Ecommerce.Application.Dtos.Producto;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _service;

        public ProductoController(IProductoService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int numeroPagina = 1, [FromQuery] int pageSize = 10)
        {
            var registros = await _service.ObtenerPaginadosAsync(numeroPagina, pageSize);
            if (registros == null || !registros.Any())
                return NotFound("No hay registros disponibles.");

            var totalRegistros = await _service.ContarAsync();

            return Ok(new RespuestaPaginada<ProductoDTO>(registros, totalRegistros, numeroPagina, pageSize));
        }

        [HttpGet("{id:int}", Name = "GetProducto")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var registro = await _service.ObtenerPorIdAsync(id);
            return Ok(registro);
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromForm] CrearProductoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(request.imagenes!.Count > 3)
                return BadRequest("No se puede enviar más de tres imágenes por producto.");

            // Subir imágenes
            var urls = new string[3] { string.Empty, string.Empty, string.Empty };

            try
            {
                for (int i = 0; i < request.imagenes.Count; i++)
                    urls[i] = await _service.GuardarArchivosAsync(request.imagenes[i],
                        "ImagenesProductos"
                    );

                // convertir el request a DTO
                var dto = new CrearProductoDTO
                {
                    nombreProducto = request.nombreProducto,
                    descripcionProducto = request.descripcionProducto,
                    precio = request.precio,
                    stock = request.stock,
                    seccion = request.seccion,
                    idCategoria = request.idCategoria,
                    imagen1 = urls[0],
                    imagen2 = urls[1],
                    imagen3 = urls[2]
                };

                var productoCreado = await _service.CrearAsync(dto);

                return CreatedAtRoute(
                    "GetProducto",
                    new { id = productoCreado.idProducto },
                    productoCreado
                );
            }

            catch
            {
                foreach(var url in urls)
                    await _service.EliminarArchivoAsync(url);

                throw;
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromForm] ActualizarProductoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var productoExistente = await _service.ObtenerPorIdAsync(id);
            if(productoExistente == null)
                return NotFound();

            var nuevasUrls = new string[3]
            {
                productoExistente.imagen1,
                productoExistente.imagen2,
                productoExistente.imagen3
            };

            // Si vienen nuevas imagenes, se reemplazan
            if(request.imagenes != null && request.imagenes.Any())
            {
                if(request.imagenes.Count > 3)
                    return BadRequest("No se puede enviar más de tres imágenes por producto.");

                for (int i = 0; i < request.imagenes.Count; i++)
                {
                    await _service.EliminarArchivoAsync(nuevasUrls[i]);

                    nuevasUrls[i] = await _service.GuardarArchivosAsync(
                        request.imagenes[i],
                        "ImagenesProductos"
                    );
                }
            }

            // Convertir el request en un DTO
            var dto = new ActualizarProductoDTO
            {
                nombreProducto = request.nombreProducto,
                descripcionProducto = request.descripcionProducto,
                precio = request.precio,
                stock = request.stock,
                seccion = request.seccion,
                idCategoria = request.idCategoria,
                imagen1 = nuevasUrls[0],
                imagen2 = nuevasUrls[1],
                imagen3 = nuevasUrls[2]
            };

            await _service.AcualizarAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DesactivarProducto(int id)
        {
            await _service.EliminarAsync(id);
            return NoContent();
        }
    }
}
