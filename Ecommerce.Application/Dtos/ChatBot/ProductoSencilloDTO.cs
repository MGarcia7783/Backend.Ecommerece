using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Dtos.ChatBot
{
    public class ProductoSencilloDTO
    {
        public int idProducto { get; set; }
        public string nombreProducto { get; set; } = string.Empty;
        public string descripcionProducto { get; set; } = string.Empty;
        public decimal precio { get; set; }
        public int stock { get; set; }
        public string imagenUrl { get; set; } = string.Empty;
    }
}
