using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Response
{
    public class RespuestaPaginada<T>
    {
        public IEnumerable<T> Elementos { get; set; } = Enumerable.Empty<T>();  // Lista de elementos en la página actual
        public int TotalPaginas { get; set; }      // Número total de páginas
        public int NumeroPagina { get; set; }      // Número de página actual
        public int TotalElementos { get; set; }
        public int TamanoPagina { get; set; }      // Número de registros por página


        public RespuestaPaginada(IEnumerable<T> elementos, int totalElementos, int numeroPagina, int tamanoPagina)
        {
            Elementos = elementos;
            TotalElementos = totalElementos;
            NumeroPagina = numeroPagina < 1 ? 1 : numeroPagina;
            TamanoPagina = tamanoPagina < 1 ? 10 : tamanoPagina;
            TotalPaginas = (int)Math.Ceiling(totalElementos / (double)tamanoPagina);
        }
    }
}
