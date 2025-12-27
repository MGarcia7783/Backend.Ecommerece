using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;
using System.Text;

namespace Ecommerce.Application.Dtos
{
    public class CrearCategoriaDTO
    {
        [Required(ErrorMessage = "El nombre de la categoría es requerido.")]
        [MaxLength(40, ErrorMessage = "El nombre no debe execeder los 40 caracteres.")]
        public string nombreCategoria { get; set; } = string.Empty;


        [Required(ErrorMessage = "La descripción de la categoría es requerida.")]
        [MaxLength(500, ErrorMessage = "La descripción no debe execeder los 500 caracteres.")]
        public string descripcionCategoria { get; set; } = string.Empty;
    }
}
