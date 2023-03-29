using AutoMapper;
using IQ_Api.DTOs;
using IQ_Api.Entidades;

namespace IQ_Api.Utils
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() { 

        CreateMap<CredencialesUsuario, CreacionUsuarioDTO>().ReverseMap();
        CreateMap<Roles,rolesDTO>().ReverseMap();
        CreateMap<rolesCreacionDTO,Roles>();
        }
    }
}
