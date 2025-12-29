using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ChatBotRepository : IChatBotRepository
    {
        private readonly EcommerceDbContext _context;

        public ChatBotRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Producto>> ObtenerProductoPorCategoriaAsync(string nombreCategoria, int top = 10)
        {
            var nombreNormalizado = nombreCategoria.Trim().ToLower();
            return await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Categoria != null && p.Categoria.nombreCategoria.ToLower() == nombreNormalizado)
                .Take(top)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerProductoPorNombreAsync(string nombreProducto)
        {
            var nombreNormalizado = nombreProducto.Trim().ToLower();
            return await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.nombreProducto.Trim().ToLower() == nombreNormalizado);
        }

        public async Task<Producto?> ObtenerProductoPorNombreODescripcionAsync(string valor)
        {
            var nombreNormalizado = valor.Trim().ToLower();

            // Buscar por nombre o descripción
            var pattern = $"%{nombreNormalizado}%";
            return await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p =>
                    EF.Functions.Like(p.nombreProducto.Trim().ToLower(), pattern) ||
                    EF.Functions.Like(p.descripcionProducto.Trim().ToLower(), pattern));
        }

        public async Task<IList<Producto>> ObtenerProductoPorSeccionAsync(string seccion, int top = 10)
        {
            var seccionNormalizada = seccion.Trim().ToLower();
            return await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.seccion != null && p.seccion.ToLower() == seccionNormalizada)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IList<Producto>> SugerirProductosPorPalabraClaveAsync(IEnumerable<string> clave, int top = 10)
        {
            var palabras = clave
                .Select(k => k.Trim().ToLower())
                .Where(k => k.Length > 0)
                .ToList();

            if(!palabras.Any())
                return new List<Producto>();

            IQueryable<Producto> query = _context.Productos.Include(p => p.Categoria);

            var predicate = PredicateBuilder.New<Producto>();

            foreach (var palabra in palabras)
            {
                var temp = palabra;
                predicate = predicate.Or(p =>
                    EF.Functions.Like(p.nombreProducto.ToLower(), "%" + temp + "%") ||
                    EF.Functions.Like(p.descripcionProducto.ToLower(), "%" + temp + "%")
                );
            }

            return await _context.Productos
                .Include(p => p.Categoria)
                .AsExpandable()         // Es necesario para EF + LinqKit
                .Where(predicate)
                .Take(top)
                .ToListAsync();
        }
    }
}
