using Ecommerce.Application.Dtos.ChatBot;
using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Application.Response;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ecommerce.Application.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IChatBotRepository _repository;

        public ChatBotService(IChatBotRepository repository)
        {
            _repository = repository; 
        }

        public async Task<ChatBotResponseDTO> ObtenerRespuestaAsync(ChatBotRequestDTO request)
        {
            var texto = (request.mensaje ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(texto))
                return new ChatBotResponseDTO { respuesta = "Por favor, ingresa una pregunta o comentario para que pueda ayudarte." };

            // 1. Detectar la inención del usuario (palabra clave)
            var textoNormalizado = texto.ToLower();
            if(ConstainsAny(textoNormalizado, new[] { "stock", "disponible", "hay", "quedan", "cantidad" }))
            {
                // Detectar el nombre con la misma frase
                var producto = await _repository.ObtenerProductoPorNombreAsync(texto) ??
                               await _repository.ObtenerProductoPorNombreODescripcionAsync(texto);

                if(producto != null)
                {
                    var stock = producto.stock;
                    return new ChatBotResponseDTO
                    {
                        respuesta = stock > 0
                            ? $"Sí, tenemos '{stock}' unidades disponibles del producto: '{producto.nombreProducto}'."
                            : $"Lo siento, el producto: '{producto.nombreProducto}' está agotado en este momento.",
                        itencion = "Consulta de Stock",
                        productosSugeridos = new List<ProductoSencilloDTO>
                        {
                            MapProducto(producto)
                        }
                    };
                }

                // Si no encuentra producto, sugerir productos relacionados
                var palabrasClave = ExtraerPalabraClave(texto);
                var sugerido = await _repository.SugerirProductosPorPalabraClaveAsync(palabrasClave);
                return new ChatBotResponseDTO
                {
                    respuesta = "No pude encontrar el producto específico que mencionaste. Sin embargo, aquí tienes algunos productos relacionados que podrían interesarte.",
                    itencion = "Consulta de Stock - Producto no Encontrado",
                    productosSugeridos = sugerido.Select(MapProducto).ToList()
                };
            }

            // 2. Buscar por coincidencia exacta o parcial del producto
            var exacto = await _repository.ObtenerProductoPorNombreAsync(texto);
            if (exacto == null)
                exacto = await _repository.ObtenerProductoPorNombreODescripcionAsync(texto);

            if(exacto != null)
            {
                return new ChatBotResponseDTO
                {
                    respuesta = $"Encontré el producto que mencionastes: '{exacto.nombreProducto}'. Actualmente tenemos '{exacto.stock}' unidades disponibles. Tiene un precio de: '{exacto.precio}'.",
                    itencion = "Producto Encontrado",
                    productosSugeridos = new List<ProductoSencilloDTO>
                    {
                        MapProducto(exacto)
                    }
                };
            }

            // 3. Buscar por categoría
            var categoria = await _repository.ObtenerProductoPorCategoriaAsync(texto);
            if(categoria.Any())
            {
                return new ChatBotResponseDTO
                {
                    respuesta = "Aquí tienes algunos productos que encontré en la categoría que mencionastes:",
                    itencion = "Productos por Categoría",
                    productosSugeridos = categoria.Take(6).Select(MapProducto).ToList()
                };
            }

            // 4. Buscar por sección
            var seccion = await _repository.ObtenerProductoPorSeccionAsync(texto);
            if(seccion.Any())
            {
                return new ChatBotResponseDTO
                {
                    respuesta = "Aquí tienes algunos productos que encontré en la sección que mencionastes:",
                    itencion = "Productos por Sección",
                    productosSugeridos = seccion.Take(6).Select(MapProducto).ToList()
                };
            }

            // 5. Sugerencias por palabra clave
            var palabrasClaveGenerales = ExtraerPalabraClave(texto);
            var sugerenciasGenerales = await _repository.SugerirProductosPorPalabraClaveAsync(palabrasClaveGenerales, top: 5);
            if(sugerenciasGenerales.Any())
            {
                return new ChatBotResponseDTO
                {
                    respuesta = "No pude encontrar un producto específico, pero aquí tienes algunas sugerencias basadas en tu consulta:",
                    itencion = "Sugerencias Generales",
                    productosSugeridos = sugerenciasGenerales.Select(MapProducto).ToList()
                };
            }

            // 6. Respuesta genérica
            return new ChatBotResponseDTO
            {
                respuesta = "Lo siento, no pude encontrar información relevante para tu consulta. Prueba buscar por nombre, categoría o preguntar por stock. ¿Podrías proporcionar más detalles o especificar el producto que estás buscando?",
                itencion = "Sin Resultados"
            };
        }

        private static bool ConstainsAny(string origen, IEnumerable<string> palabraClave)
        {
            return palabraClave.Any(p => origen.Contains(p));
        }

        private static IEnumerable<string> ExtraerPalabraClave(string texto)
        {
            var palabras = Regex.Split(texto.ToLower(), @"\W+")
                .Where(p => p.Length >= 2)
                .Except(new[] { "el", "la", "los", "las", "un", "una", "unos", "unas", "de", "del", "y", "o", "a", "en", "con", "por", "para", "es", "son", "que", "qué" });

            return palabras;
        }

        private static ProductoSencilloDTO MapProducto(Producto producto)
        {
            return new ProductoSencilloDTO
            {
                idProducto = producto.idProducto,
                nombreProducto = producto.nombreProducto,
                descripcionProducto = producto.descripcionProducto,
                precio = producto.precio,
                stock = producto.stock,
                imagenUrl = producto.imagen1
            };
        }
    }
}
