using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecommerce.Application.Dtos.Usuario
{
    public class CrearUsuarioDTO
    {
        [Required(ErrorMessage = "El nombre del usuario es requerido.")]
        public string nombreCompleto { get; set; } = string.Empty;


        [Required(ErrorMessage = "El email del usuario es requerido.")]
        public string UserName { get; set; } = string.Empty;


        [Required(ErrorMessage = "La contraseña del usuario es requerida.")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "El rol del usuario es requerido.")]
        public string Rol { get; set; } = string.Empty;
    }
}
