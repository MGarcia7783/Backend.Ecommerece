using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuarioRepository(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<int> ContarUsuariosAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        public async Task<IdentityResult> CrearAsync(Usuario usuario, string password)
        {
            return await _userManager
                .CreateAsync(usuario, password);
        }

        public async Task<bool> ExisteUserNameAsync(string username)
        {
            var nombreNormalizado = username.Trim().ToLower();

            return await _userManager.Users
                .AnyAsync(x => x.UserName!.Trim().ToLower() == nombreNormalizado);
        }

        public async Task<ICollection<Usuario>> ObtenerPaginadosAsync(int pagina, int tamano)
        {
            if (pagina <= 0) pagina = 1;
            if (tamano <= 0) tamano = 10;

            return await _userManager.Users
                .OrderBy(x => x.nombreCompeto)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<Usuario?> ObtenerPorUserNameAsync(string username)
        {
            var usuario = await _userManager
                .FindByNameAsync(username);

            return usuario;
        }

        public async Task<bool> VerificarPasswordAsync(Usuario usuario, string password)
        {
            return await _userManager
                .CheckPasswordAsync(usuario, password);
        }
    }
}
