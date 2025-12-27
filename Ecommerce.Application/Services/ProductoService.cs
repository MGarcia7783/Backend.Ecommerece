using AutoMapper;
using Ecommerce.Application.Dtos.Producto;
using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _http;

        public ProductoService(IProductoRepository repository, IMapper mapper, IHttpContextAccessor http)
        {
            _repository = repository;
            _mapper = mapper;
            _http = http;
        }

        public async Task<bool> AcualizarAsync(int id, ActualizarProductoDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException("El cuerpo de la solicitud no puede ser nulo.");

            if (id <= 0)
                throw new ArgumentOutOfRangeException("El ID debe ser mayor a cero.");

            var registroActual = await _repository.ObtenerPorIdAsync(id);
            if (registroActual == null)
                throw new KeyNotFoundException($"El producto con ID: '{id}' no existe o fue eliminado.");

            bool cambioNombre =
                !string.Equals(registroActual.nombreProducto.Trim(), dto.nombreProducto.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (cambioNombre && await _repository.ExisteNombreAsync(dto.nombreProducto))
                throw new InvalidOperationException($"Ya existe un producto con el nombre: '{dto.nombreProducto}'.");

            _mapper.Map(dto, registroActual);
            return await _repository.AcualizarAsync(registroActual);
        }

        public async Task<int> ContarAsync()
        {
            return await _repository.ContarActivosAsync();
        }

        public async Task<ProductoDTO> CrearAsync(CrearProductoDTO dto)
        {
            if (dto == null)
                throw new InvalidOperationException("Datos inválidos.");

            string nombreNomarlizado = dto.nombreProducto.Trim();

            if (await _repository.ExisteNombreAsync(nombreNomarlizado))
                throw new InvalidOperationException($"Ya existe un producto con el nombre: '{dto.nombreProducto}'.");

            var producto = _mapper.Map<Producto>(dto);
            await _repository.CrearAsync(producto);

            var productoCreado = await _repository.ObtenerPorIdAsync(producto.idProducto);

            return _mapper.Map<ProductoDTO>(productoCreado);
        }

        public Task EliminarArchivoAsync(string urlImagen)
        {
            if (string.IsNullOrWhiteSpace(urlImagen))
                return Task.CompletedTask;

            // Soporta URLS absolutas y relativas
            string rutaRelativa;
            if(Uri.TryCreate(urlImagen, UriKind.Absolute, out var uri))
                rutaRelativa = uri.LocalPath.TrimStart('/');
            else
                rutaRelativa = urlImagen.TrimStart('/');

            var rutaFisica = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                rutaRelativa
            );

            if(File.Exists(rutaFisica))
                File.Delete(rutaFisica);

            return Task.CompletedTask;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _repository.ObtenerPorIdAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"el producto con ID: '{id}' no existe o fue eliminado.");

            return await _repository.DesactivarAsync(producto);
        }

        public async Task<string> GuardarArchivosAsync(IFormFile archivo, string carpeta)
        {
            var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", carpeta);

            if(!Directory.Exists(carpetaDestino))
                Directory.CreateDirectory(carpetaDestino);

            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var ruta = Path.Combine(carpetaDestino, nombreArchivo);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Construir URL pública
            var request = _http.HttpContext!.Request;
            return $"{request.Scheme}://{request.Host}/{carpeta}/{nombreArchivo}";
        }

        public async Task<ICollection<ProductoDTO>> ObtenerPaginadosAsync(int pagina, int tamano)
        {
            var productos = await _repository.ObtenerPaginadosAsync(pagina, tamano);
            return _mapper.Map<ICollection<ProductoDTO>>(productos);
        }

        public async Task<ProductoDTO?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.");

            var producto = await _repository.ObtenerPorIdAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"No se encontró el producto con ID: '{id}'.");

            return _mapper.Map<ProductoDTO>(producto);
        }
    }
}
