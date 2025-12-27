using AutoMapper;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Api.Mapping
{
    public class MappingsProfile : Profile
    {
        public MappingsProfile()
        {
            #region Mapeo del modelo categoria

            CreateMap<Categoria, CategoriaDTO>().ReverseMap();
            CreateMap<CrearCategoriaDTO, Categoria>();
            CreateMap<ActualizarCategoriaDTO, Categoria>()
                .ForMember(dest => dest.estado, opt => opt.Ignore())
                .ForMember(dest => dest.fechaRegistro, opt => opt.Ignore());

            #endregion
        }
    }
}
