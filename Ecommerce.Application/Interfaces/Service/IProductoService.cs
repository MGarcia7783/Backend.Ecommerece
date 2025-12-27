using Ecommerce.Application.Dtos.Producto;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Interfaces.Service
{
    public interface IProductoService
    {
        Task<ICollection<ProductoDTO>> ObtenerPaginadosAsync(int pagina, int tamano);
        Task<int> ContarAsync();
        Task<ProductoDTO?> ObtenerPorIdAsync(int id);
        Task<ProductoDTO> CrearAsync(CrearProductoDTO dto);
        Task<bool> AcualizarAsync(int id, ActualizarProductoDTO dto);
        Task<string> GuardarArchivosAsync(IFormFile archivo, string carpeta);
        Task EliminarArchivoAsync(string urlImagen);
        Task<bool> EliminarAsync(int id);
    }
}
