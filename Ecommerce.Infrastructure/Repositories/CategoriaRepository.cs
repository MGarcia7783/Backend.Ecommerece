using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly EcommerceDbContext _context;

        public CategoriaRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcualizarAsync(Categoria categoria)
        {
            var registroActual = await _context.Categorias.FindAsync(categoria.idCategoria);
            if (registroActual == null)
                return false;

            _context.Entry(registroActual).CurrentValues.SetValues(categoria);
            return await GuardarCambiosAsync();
        }

        public async Task<int> ContarActivosAsync()
        {
            return await _context.Categorias
                .CountAsync(x => x.estado == "Activo");
        }

        public async Task<bool> CrearAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            return await GuardarCambiosAsync();
        }

        public async Task<bool> DesactivarAsync(Categoria categoria)
        {
            categoria.estado = "Inactivo";
            _context.Categorias.Update(categoria);

            return await GuardarCambiosAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            var nombreNormalizado = nombre.Trim().ToLower();

            return await _context.Categorias
                .AnyAsync(x => x.nombreCategoria.Trim().ToLower() == nombreNormalizado);
        }

        public async Task<bool> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ICollection<Categoria>> ObtenerPaginadosAsync(int pagina, int tamano)
        {
            if(pagina <= 0) pagina = 1;
            if (tamano <= 0) tamano = 10;

            return await _context.Categorias
                .Where(x => x.estado == "Activo")
                .OrderBy(x => x.nombreCategoria)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.idCategoria == id && x.estado == "Activo");
        }

        public async Task<bool> TieneProductosActivosAsync(int id)
        {
            return await _context.Productos
                .AnyAsync(p => p.idCategoria == id && p.estado == "Activo");
        }
    }
}
