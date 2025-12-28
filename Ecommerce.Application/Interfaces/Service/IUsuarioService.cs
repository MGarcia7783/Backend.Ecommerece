using Ecommerce.Application.Dtos.Usuario;
using Ecommerce.Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Interfaces.Service
{
    public interface IUsuarioService
    {
        Task<UsuarioDTO> CrearUsuarioAsync(CrearUsuarioDTO dto);
        Task<LoginRespuestaUsuarioDTO> LogingAsync(LoginUsuarioDTO dto);
        Task<UsuarioDTO?> ObtenerPorIdAsync(string id);
        Task<ICollection<UsuarioDTO>> ObtenerPaginadosAsync(int pagina, int tamano);
        Task<int> ContarAsync();        
    }
}
