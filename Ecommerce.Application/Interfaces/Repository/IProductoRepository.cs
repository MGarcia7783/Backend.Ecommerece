using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Interfaces.Repository
{
    public interface IProductoRepository
    {
        Task<ICollection<Producto>> ObtenerPaginadosAsync(int pagina, int tamano);
        Task<int> ContarActivosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteNombreAsync(string nombre);
        Task<bool> CrearAsync(Producto producto);
        Task<bool> AcualizarAsync(Producto producto);
        Task<bool> DesactivarAsync(Producto producto);
        Task<bool> GuardarCambiosAsync();
    }
}
