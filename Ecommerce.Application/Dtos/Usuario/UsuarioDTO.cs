using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecommerce.Application.Dtos.Usuario
{
    public class UsuarioDTO
    {
        public string Id { get; set; } = string.Empty;
        public string nombreCompleto { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
