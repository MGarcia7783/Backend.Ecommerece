using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Service
{
    public interface ICategoriaService
    {
        Task<ICollection<CategoriaDTO>> ObtenerPaginadosAsync(int pagina, int tamano);
        Task<int> ContarActivosAsync();
        Task<CategoriaDTO?> ObtenerPorIdAsync(int id);
        Task<CategoriaDTO> CrearAsync(CrearCategoriaDTO dto);
        Task<bool> AcualizarAsync(int id, ActualizarCategoriaDTO dto);
        Task<bool> DesactivarAsync(int id);
    }
}
