using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Interfaces.Repository
{
    public interface ICategoriaRepository
    {
        Task<ICollection<Categoria>> ObtenerPaginadosAsync(int pagina, int tamano);
        Task<int> ContarActivosAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteNombreAsync(string nombre);
        Task<bool> CrearAsync(Categoria categoria);
        Task<bool> AcualizarAsync(Categoria categoria);
        Task<bool> DesactivarAsync(Categoria categoria);
        Task<bool> TieneProductosActivosAsync(int id);
        Task<bool> GuardarCambiosAsync();
    }
}
