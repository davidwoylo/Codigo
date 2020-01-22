using CFC_Negocio.Exceptions;
using CFC_Negocio.Servicos;
using NLog;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace CFC_Controlador
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExceptionHandler : ExceptionFilterAttribute
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        EnvioEmail email = new EnvioEmail();

        public override void OnException(HttpActionExecutedContext context)
        {
            var ex = GetCorrectException(context.Exception);
            if (ex is KnownException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message)
                });
            }

            if (ex is DbEntityValidationException)
            {
                DbEntityValidationException e = ex as DbEntityValidationException;

                string str = "";

                foreach (var eve in e.EntityValidationErrors)
                {
                    str += $"Entity do tipo \"{eve.Entry.Entity.GetType().Name}\" no estado \"{eve.Entry.State}\" possuí os seguintes erros de validação:";
                    foreach (var ve in eve.ValidationErrors)
                    {
                        string tmpString = $"- Propriedade: \"{ve.PropertyName}\", Erro: \"{ve.ErrorMessage}\"";
                        str += tmpString;
                    }
                }

                logger.Info(ex, str);

                email.EnviarEmail($"Erro de entidade do banco de dados: {str}", ex.GetType().ToString(), ex.Source?.ToString(), ex.StackTrace?.ToString(), ex.Data?.ToString(), ex.TargetSite?.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Erro de entidade do banco de dados: {str}")
                });
            }
            
            
            if (ex is DbUpdateException)
            {
                logger.Info(ex, ex.Message);

                email.EnviarEmail("Esse registro não pode ser excluído por possuir dados vinculados!", ex.GetType().ToString(), ex.Source?.ToString(), ex.StackTrace?.ToString(), ex.Data?.ToString(), ex.TargetSite?.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Esse registro não pode ser excluído por possuir dados vinculados!")
                });
            }

            if (ex is WebException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("Usuário não autorizado!")
                });
            }

            if (ex is UnauthorizedAccessException)
            {
                email.EnviarEmail($"{ex.Message}", ex.GetType().ToString(), ex.Source?.ToString(), ex.StackTrace?.ToString(), ex.Data?.ToString(), ex.TargetSite?.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message)
                });
            }

            //Log Critical errors
            logger.Error(ex, ex.Message );
            //Debug.WriteLine(ex);
            if (ex.Message != "Uma tarefa foi cancelada.") {
                email.EnviarEmail($"Erro interno do servidor. \n {ex.Message}", ex.GetType().ToString(), ex.Source?.ToString(), ex.StackTrace?.ToString(), ex.Data?.ToString(), ex.TargetSite?.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    //Content = new StringContent($"Erro desconhecido.\r\n {ex.Message}")
                    Content = new StringContent($"Erro interno do servidor.")
                });
            }
            
        }

        public Exception GetCorrectException(Exception exception)
        {
            if (exception.InnerException != null)
            {
                return GetCorrectException(exception.InnerException);
            }
            else
            {
                return exception;
            }
        }
    }
}