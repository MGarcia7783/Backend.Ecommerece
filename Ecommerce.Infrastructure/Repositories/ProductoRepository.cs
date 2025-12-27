using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly EcommerceDbContext _context;

        public ProductoRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcualizarAsync(Producto producto)
        {
            var registroActual = await _context.Productos.FindAsync(producto.idProducto);
            if (registroActual == null)
                return false;

            _context.Entry(registroActual).CurrentValues.SetValues(producto);
            return await GuardarCambiosAsync();
        }

        public async Task<int> ContarActivosAsync()
        {
            return await _context.Productos
                .CountAsync(x => x.estado == "Activo");
        }

        public async Task<bool> CrearAsync(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
            return await GuardarCambiosAsync();
        }

        public async Task<bool> DesactivarAsync(Producto producto)
        {
            producto.estado = "Inactivo";
            _context.Productos.Update(producto);

            return await GuardarCambiosAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            var nombreNormalizado = nombre.Trim().ToLower();

            return await _context.Productos
                .AnyAsync(x => x.nombreProducto.Trim().ToLower() == nombreNormalizado);
        }

        public async Task<bool> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ICollection<Producto>> ObtenerPaginadosAsync(int pagina, int tamano)
        {
            if(pagina <= 0) pagina = 1;
            if (tamano <= 0) tamano = 10;

            return await _context.Productos
                .Where(x => x.estado == "Activo")
                .OrderBy(x => x.nombreProducto)
                .Include(x => x.Categoria)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(x => x.Categoria)
                .FirstOrDefaultAsync(x => x.idProducto == id && x.estado == "Activo");
        }
    }
}
