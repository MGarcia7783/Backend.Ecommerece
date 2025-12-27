using AutoMapper;
using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Repository;
using Ecommerce.Application.Interfaces.Service;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;
        private readonly IMapper _mapper;

        public CategoriaService(ICategoriaRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<bool> AcualizarAsync(int id, ActualizarCategoriaDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException("El cuerpo de la solicitud no puede ser nulo.");

            if (id <= 0)
                throw new ArgumentOutOfRangeException("El ID debe ser un número entero mayor a cero.");

            var registroActual = await _repository.ObtenerPorIdAsync(id);
            if (registroActual == null)
                throw new KeyNotFoundException($"El registro con ID: '{id}' no existe o fue eliminado.");

            // Validar duplicados solo si hay cambios
            if(!string.Equals(registroActual.nombreCategoria.Trim(), dto.nombreCategoria.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (await _repository.ExisteNombreAsync(dto.nombreCategoria))
                    throw new InvalidOperationException($"Ya existe un registro con el nombre: '{dto.nombreCategoria}'.");
            }

            _mapper.Map(dto, registroActual);
            return await _repository.AcualizarAsync(registroActual);
        }

        public async Task<int> ContarActivosAsync()
        {
            return await _repository.ContarActivosAsync();
        }

        public async Task<CategoriaDTO> CrearAsync(CrearCategoriaDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException("Datos inválidos.");

            var nombreNormalizado = dto.nombreCategoria.Trim();

            if (await _repository.ExisteNombreAsync(nombreNormalizado))
                throw new InvalidOperationException($"Ya existe un registro con el nombre: '{dto.nombreCategoria}'.");

            var nuevoRegistro = _mapper.Map<Categoria>(dto);
            await _repository.CrearAsync(nuevoRegistro);

            return _mapper.Map<CategoriaDTO>(nuevoRegistro);
        }

        public async Task<bool> DesactivarAsync(int id)
        {
            var registro = await _repository.ObtenerPorIdAsync(id);
            if (registro == null)
                throw new KeyNotFoundException($"El registro con ID: {id} no existe o fue eliminado.");

            // Validar si tiene productos activos asociados
            bool tieneProductosActivos = await _repository.TieneProductosActivosAsync(id);
            if (tieneProductosActivos)
                throw new InvalidOperationException("No se puede eliminar la categoría porque tiene productos asociados.");

            return await _repository.DesactivarAsync(registro);
        }

        public async Task<ICollection<CategoriaDTO>> ObtenerPaginadosAsync(int pagina, int tamano)
        {
            var registros = await _repository.ObtenerPaginadosAsync(pagina, tamano);
            return _mapper.Map<ICollection<CategoriaDTO>>(registros);
        }

        public async Task<CategoriaDTO?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser un número entero mayor a cero.");

            var registro = await _repository.ObtenerPorIdAsync(id);
            if (registro == null)
                throw new KeyNotFoundException($"No se encontró el registro con ID: '{id}'.");

            return _mapper.Map<CategoriaDTO>(registro);
        }
    }
}
