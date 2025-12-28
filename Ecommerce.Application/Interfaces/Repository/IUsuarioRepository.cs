using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Interfaces.Repository
{
    public interface IUsuarioRepository
    {
        Task<ICollection<Usuario>> ObtenerPaginadosAsync(int pagina, int tamano);
        Task<int> ContarUsuariosAsync();
        Task<Usuario?> ObtenerPorIdAsync(string id);
        Task<bool> ExisteUserNameAsync(string username);
        Task<Usuario?> ObtenerPorUserNameAsync(string username);
        Task<IdentityResult> CrearAsync(Usuario usuario, string password);
        Task<bool> VerificarPasswordAsync(Usuario usuario, string password);
    }
}
