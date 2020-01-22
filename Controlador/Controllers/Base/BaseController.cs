using Autenticacao.DTO;
using CFC_Negocio.DTO.Ext;
using CFC_Negocio.Servicos;
using CFC_Negocio.Util;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace CFC_Controlador.Controllers.Base
{
    public abstract class BaseController<E, R, LR, F> : BaseController
        where E : class
        where R : class
        where LR : class
        where F : class
    {
        public MethodInfo filtroMethod = typeof(FiltroServico).GetMethod("Filtro", typeof(F))?.MakeGenericMethod(typeof(E));

        
        [Route("obterTodos")]
        public virtual PaginacaoDTO<LR> GetObterTodos([FromUri]F filtro, [FromUri]PaginacaoConfigDTO paginacaoConfig)
        {
            IQueryable<E> busca = (IQueryable<E>)filtroMethod?.Invoke(servico, new[] { filtro }) ?? servico.ObterTodos<E>(includes);

            return busca.AsPaginado<LR>(paginacaoConfig);
        }

        
        [Route("listarTodos")]
        public virtual List<R> GetListarTodos([FromUri]F filtro)
        {
            IQueryable<E> busca = (IQueryable<E>)filtroMethod?.Invoke(servico, new[] { filtro }) ?? servico.ObterTodos<E>(includes);
            return busca.AsList<R>();
        }

        
        [Route("{id}")]
        public virtual R GetObter(int id)
        {
            return servico.Obter<E>(id, includes).As<R>();
        }

        //
        [Route]
        public virtual HttpResponseMessage PostSalvar(R dto)
        {
            MethodInfo customValidation = typeof(FiltroServico).GetMethod("ValidarSalvar", typeof(R));
            if (customValidation != null)
            {
                customValidation.MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }
            else
            {
                typeof(FiltroServico).GetMethod("PadraoValidarSalvar", typeof(object)).MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }

            servico.Salvar<E>(dto);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        
        [Route]
        public virtual HttpResponseMessage PutAtualizar(R dto, [FromUri]string[] parcial = null)
        {
            MethodInfo customValidation = typeof(FiltroServico).GetMethod("ValidarAtualizar", typeof(R));
            if (customValidation != null)
            {
                customValidation.MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }
            else
            {
                typeof(FiltroServico).GetMethod("PadraoValidarAtualizar", typeof(object)).MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }

            servico.Atualizar<E>(dto, parcial);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        
        [Route("{id}")]
        public virtual HttpResponseMessage Delete(int id)
        {
            servico.Excluir<E>(id);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }


    }

    public abstract class BaseController<E, R, F> : BaseController
        where E : class
        where R : class
        where F : class
    {
        public MethodInfo filtroMethod = typeof(FiltroServico).GetMethod("Filtro", typeof(F))?.MakeGenericMethod(typeof(E));

        
        [Route("obterTodos")]
        public virtual PaginacaoDTO<R> GetObterTodos([FromUri]F filtro, [FromUri]PaginacaoConfigDTO paginacaoConfig)
        {
            IQueryable<E> busca = (IQueryable<E>)filtroMethod?.Invoke(servico, new[] { filtro }) ?? servico.ObterTodos<E>(includes);

            return busca.AsPaginado<R>(paginacaoConfig);
        }

        
        [Route("listarTodos")]
        public virtual List<R> GetListarTodos([FromUri]F filtro)
        {
            IQueryable<E> busca = (IQueryable<E>)filtroMethod?.Invoke(servico, new[] { filtro }) ?? servico.ObterTodos<E>(includes);
            return busca.AsList<R>();
        }

        
        [Route("{id}")]
        public virtual R GetObter(int id)
        {
            return servico.Obter<E>(id, includes).As<R>();
        }

        
        [Route]
        public virtual HttpResponseMessage PostSalvar(R dto)
        {
            MethodInfo customValidation = typeof(FiltroServico).GetMethod("ValidarSalvar", typeof(R));
            if (customValidation != null)
            {
                customValidation.MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }
            else
            {
                typeof(FiltroServico).GetMethod("PadraoValidarSalvar", typeof(object)).MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }
            
            servico.Salvar<E>(dto);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        
        [Route]
        public virtual HttpResponseMessage PutAtualizar(R dto, [FromUri]string[] parcial = null)
        {
            MethodInfo customValidation = typeof(FiltroServico).GetMethod("ValidarAtualizar", typeof(R));
            if (customValidation != null)
            {
                customValidation.MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }
            else
            {
                typeof(FiltroServico).GetMethod("PadraoValidarAtualizar", typeof(object)).MakeGenericMethod(typeof(E)).Invoke(servico, new[] { dto });
            }

            servico.Atualizar<E>(dto, parcial);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        
        [Route("{id}")]
        public virtual HttpResponseMessage Delete(int id)
        {
            servico.Excluir<E>(id);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }


    }

    public abstract class BaseController<R> : BaseController
        where R : class
    {
        
        [Route("obterTodos")]
        public virtual PaginacaoDTO<R> GetObterTodos(string classType, [FromUri]PaginacaoConfigDTO paginacaoConfig)
        {
            return new FiltroServico(classType).ObterTodos(null, includes).AsPaginado<R>(paginacaoConfig);
        }

        
        [Route("listarTodos")]
        public virtual List<R> GetListarTodos(string classType)
        {
            return new FiltroServico(classType).ObterTodos(null, includes).AsList<R>();
        }

        
        [Route("{id}")]
        public virtual R GetObter(string classType, int id)
        {
            return new FiltroServico(classType).Obter(id, null, includes).As<R>();
        }

        
        [Route]
        public virtual HttpResponseMessage PostSalvar(string classType, R dto)
        {
            new FiltroServico(classType).Salvar(dto);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        
        [Route]
        public virtual HttpResponseMessage PutAtualizar(string classType, R dto, [FromUri]string[] parcial = null)
        {
            new FiltroServico(classType).Atualizar(dto, parcial);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        
        [Route("{id}")]
        public virtual HttpResponseMessage Delete(string classType, int id)
        {
            new FiltroServico(classType).Excluir(id);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }
    }

    public class BaseController : ApiController
    {
        public string[] includes = null;

        public FiltroServico servico = new FiltroServico();
        
        public ValidacaoDocumentoServico validarDocumentos = new ValidacaoDocumentoServico();

        private UsuarioDTO _usuarioLogado = null;

        public UsuarioDTO usuarioLogado
        {
            get
            {
                var token = Request?.GetQueryNameValuePairs().FirstOrDefault(x => x.Key == "token" && x.Value != null);
                if (token == null && Request?.Headers?.Authorization == null)
                    return null;
                return new SSOService().Permissao(token?.Value ?? Request?.Headers?.Authorization?.ToString());
            }
        }
    }
}