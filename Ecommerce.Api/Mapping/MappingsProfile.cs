using AutoMapper;
using Ecommerce.Application.Dtos.Categoria;
using Ecommerce.Application.Dtos.Producto;
using Ecommerce.Application.Dtos.Usuario;
using Ecommerce.Application.Response;
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

            #region Mapeo del modelo producto

            CreateMap<Producto, ProductoDTO>()
                .ForMember(dest => dest.nombreCategoria,
                            opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.nombreCategoria : string.Empty));

            CreateMap<CrearProductoDTO, Producto>();
            CreateMap<ActualizarProductoDTO, Producto>()
                .ForMember(dest => dest.Categoria, opt => opt.Ignore())
                .ForMember(dest => dest.estado, opt => opt.Ignore())
                .ForMember(dest => dest.fechaRegistro, opt => opt.Ignore());

            #endregion

            #region Mapeo del modelo usuario

            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.Rol, opt => opt.Ignore());
            CreateMap<CrearUsuarioDTO, Usuario>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.nombreCompeto, opt => opt.MapFrom(src => src.nombreCompleto));

            CreateMap<Usuario, LoginRespuestaUsuarioDTO>()
                .ForMember(dest => dest.Usuario, opt => opt.MapFrom(src => src));

            #endregion
        }
    }
}
