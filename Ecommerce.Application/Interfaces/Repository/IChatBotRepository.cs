using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repository
{
    public interface IChatBotRepository
    {
        Task<Producto?> ObtenerProductoPorNombreAsync(string nombreProducto);
        Task<Producto?> ObtenerProductoPorNombreODescripcionAsync(string valor);
        Task<IList<Producto>> ObtenerProductoPorCategoriaAsync(string nombreCategoria, int top = 10);
        Task<IList<Producto>> ObtenerProductoPorSeccionAsync(string seccion, int top = 10);
        Task<IList<Producto>> SugerirProductosPorPalabraClaveAsync(IEnumerable<string> clave, int top = 10);
    }
}
