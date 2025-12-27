using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Dtos.Producto
{
    public class CrearProductoDTO
    {
        public string nombreProducto { get; set; } = string.Empty;
        public string descripcionProducto { get; set; } = string.Empty;
        public decimal precio { get; set; }
        public int stock { get; set; }
        public string seccion { get; set; } = string.Empty;
        public int idCategoria { get; set; }
        public string imagen1 { get; set; } = string.Empty;
        public string imagen2 { get; set; } = string.Empty;
        public string imagen3 { get; set; } = string.Empty;
    }
}
