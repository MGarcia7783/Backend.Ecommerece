using AutoMapper;
using Ecommerce.Application.Dtos.Usuario;
using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Response;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public UsuarioService(IUsuarioRepository repository, UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IConfiguration config)
        {
            _repository = repository;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _config = config;
        }

        #region Métodos

        private async Task<UsuarioDTO> MapearUsuarioDTOAsync(Usuario usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var rol = roles.FirstOrDefault() ?? string.Empty;

            return new UsuarioDTO
            {
                Id = usuario.Id,
                UserName = usuario.UserName!,
                nombreCompleto = usuario.nombreCompeto,
                Rol = rol
            };
        }


        public string GenerarToken(Usuario usuario, string rol)
        {
            // Leer configuración de variables de entorno
            var key = _config["JWT_KEY"]
                ?? throw new Exception("JWT_KEY no está configurada en el entorno.");
            var issuer = _config["JWT_ISSUER"] ?? "EcommerceApi";
            var audience = _config["JWT_AUDIENCE"] ?? "EcommerceApiUsers";


            // Convertir la clave secreta a bytes
            // Esto es necesario para crear la firma del token
            var keyBytes = Encoding.ASCII.GetBytes(key);


            // Crear claims (información que incluirá el token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Name, usuario.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, rol)
            };


            // Preparar el descriptor del token
            // Aquí se define la información del token: claims, expiración, issuer, audience y firma
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),           // Claim que incluirá el token
                Expires = DateTime.UtcNow.AddDays(7),           // El token expira en 7 días
                Issuer = issuer,                                // Quién emite el token
                Audience = audience,                            // A quien va dirigido el token
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),         // Clave secreta en bytes
                    SecurityAlgorithms.HmacSha256Signature)     // Algoritmo de firma HMAC SHA256
            };

            // Generar el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Devolver el token como string
            return tokenHandler.WriteToken(token);
        }

        #endregion

        public async Task<int> ContarAsync()
        {
            return await _repository.ContarUsuariosAsync();
        }

        public async Task<UsuarioDTO> CrearUsuarioAsync(CrearUsuarioDTO dto)
        {
            if (dto == null)
                throw new InvalidOperationException("Daros son inválidos.");

            // Validar si existe email/username
            var existeEmail = await _repository.ObtenerPorUserNameAsync(dto.UserName);
            if (existeEmail != null)
                throw new InvalidOperationException("El email digitado ya ha sido registrado.");

            // Validar el rol
            var rolExiste = await _roleManager.RoleExistsAsync(dto.Rol);
            if (!rolExiste)
                throw new InvalidOperationException("El rol especificado no existe.");

            // Mapear a entidad usuario
            var usuario = _mapper.Map<Usuario>(dto);

            // Crear usuario con Identity
            var usuarioCreado = await _repository.CrearAsync(usuario, dto.Password);

            if(!usuarioCreado.Succeeded)
            {
                var errores = string.Join(" | ", usuarioCreado.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario: '{errores}'");
            }

            // Asignar rol
            var usuarioRol = await _userManager.AddToRoleAsync(usuario, dto.Rol);

            if(!usuarioRol.Succeeded)
            {
                var errores = string.Join(" | ", usuarioRol.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"No se pudo asignar el rol: '{errores}'");
            }

            // Retornar un DTO
            return await MapearUsuarioDTOAsync(usuario);
        }

        public async Task<LoginRespuestaUsuarioDTO> LogingAsync(LoginUsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException("Los datos del login son requeridos.");

            var usuario = await _repository.ObtenerPorUserNameAsync(dto.UserName);

            if (usuario == null)
                throw new UnauthorizedAccessException("El email digitado no se encuentra registrado.");

            if(!await _repository.VerificarPasswordAsync(usuario, dto.Password))
                throw new UnauthorizedAccessException("Contraseña incorrecta. Verifique por favor.");

            // Obtener el rol del usuario
            var usuarioDTO = await MapearUsuarioDTOAsync(usuario);

            return new LoginRespuestaUsuarioDTO
            {
                Usuario = usuarioDTO,
                Token = GenerarToken(usuario, usuarioDTO.Rol)
            };
        }

        public async Task<ICollection<UsuarioDTO>> ObtenerPaginadosAsync(int pagina, int tamano)
        {
            var usuarios = await _repository.ObtenerPaginadosAsync(pagina, tamano);
            var lista = new List<UsuarioDTO>();

            foreach (var usuario in usuarios)
            {
                lista.Add(await MapearUsuarioDTOAsync(usuario));
            }

            return lista;
        }

        public async Task<UsuarioDTO?> ObtenerPorIdAsync(string id)
        {
            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID: '{id}'.");

            return await MapearUsuarioDTOAsync(usuario);
        }
    }
}
