using Ecommerce.Application.Dtos.Categoria;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ecommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int numeroPagina = 1, [FromQuery] int pageSize = 10)
        {
            var registros = await _service.ObtenerPaginadosAsync(numeroPagina, pageSize);
            if( registros == null || !registros.Any())
                return NotFound("No hay registros disponibles.");

            var totalRegistros = await _service.ContarActivosAsync();

            return Ok(new RespuestaPaginada<CategoriaDTO>(registros, totalRegistros, numeroPagina, pageSize));
        }

        [HttpGet("{id:int}", Name = "GetCategoria")]
        public async Task<IActionResult> GetCategoria(int id)
        {
            var registro = await _service.ObtenerPorIdAsync(id);
            return Ok(registro);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDTO dto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var registro = await _service.CrearAsync(dto);
            return CreatedAtRoute(
                "GetCategoria",
                new { id = registro.idCategoria },
                registro
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] ActualizarCategoriaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.AcualizarAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DesactivarCategoria(int id)
        {
            await _service.DesactivarAsync(id);
            return NoContent();
        }
    }
}
