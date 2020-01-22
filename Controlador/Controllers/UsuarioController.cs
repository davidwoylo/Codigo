using Autenticacao.DTO;
using CFC_Controlador.Controllers.Base;
using CFC_Entidades;
using CFC_Negocio.DTO.Filtros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CFC_Controlador.Controllers
{
    [RoutePrefix("usuario")]
    public class UsuarioController : BaseController<Usuario, UsuarioDTO, UsuarioFiltroDTO>
    {
        public UsuarioController() { }
    }
}
