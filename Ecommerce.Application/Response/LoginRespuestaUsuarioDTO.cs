using Ecommerce.Application.Dtos.Usuario;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Response
{
    public class LoginRespuestaUsuarioDTO
    {
        public UsuarioDTO? Usuario { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
