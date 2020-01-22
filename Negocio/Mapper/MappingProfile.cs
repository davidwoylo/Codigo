using AutoMapper;
using BoletoNet;
using CFC_Entidades;
using CFC_Negocio.DTO;
using CFC_Negocio.DTO.Ext;
using CFC_Negocio.Exceptions;
using CFC_Negocio.Servicos;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace CFC_Negocio.Mapper
{
    public class MappingProfile : Profile
    {
        public FiltroServico servico = new FiltroServico();
        public MappingProfile()
        {
            CreateMap<Usuario, UsuarioDTO>().ReverseMap();
            
        }
    }
}
