using Ecommerce.Application.Dtos.Usuario;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuarioController(IUsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int numeroPagina = 1, [FromQuery] int pageSize = 10)
        {
            var registros = await _service.ObtenerPaginadosAsync(numeroPagina, pageSize);
            if (registros == null || !registros.Any())
                return NotFound("No hay usuarios disponibles.");

            var totalRegistros = await _service.ContarAsync();

            return Ok(new RespuestaPaginada<UsuarioDTO>(registros, totalRegistros, numeroPagina, pageSize));
        }

        [HttpGet("{id}", Name = "GetUsuario")]
        public async Task<IActionResult> GetUsuario(string id)
        {
            var registro = await _service.ObtenerPorIdAsync(id);
            return Ok(registro);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var registro = await _service.CrearUsuarioAsync(dto);
            return CreatedAtRoute(
                "GetUsuario",
                new { id = registro.Id },
                registro
            );
        }

        [HttpPost("login")]
        [Authorize(Roles ="Administrador, Cliente")]
        public async Task<ActionResult<LoginRespuestaUsuarioDTO>> Login([FromBody] LoginUsuarioDTO loginDTO)
        {
            return await _service.LogingAsync(loginDTO);
        }
    }
}
