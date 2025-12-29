using Ecommerce.Application.Dtos.ChatBot;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Response
{
    public class ChatBotResponseDTO
    {
        public string respuesta { get; set; } = string.Empty;
        public List<ProductoSencilloDTO> productosSugeridos { get; set; } = new List<ProductoSencilloDTO>();
        public string? itencion { get; set; }
    }
}
