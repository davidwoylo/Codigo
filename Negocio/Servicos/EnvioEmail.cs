using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CFC_Negocio.Servicos
{
    public class EnvioEmail
    {
        /// <summary>
        /// Método criado para enviar e-mail referente a exception gerada no sistema.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="getType"></param>
        /// <param name="source"></param>
        /// <param name="stackTrace"></param>
        /// <param name="data"></param>
        /// <param name="targetSite"></param>
        /// <param name="ParamName"></param>
        public void EnviarEmail(string msg = null, string getType = null, string source = null, string stackTrace = null, string data = null, string targetSite = null, object ParamName = null)
        {
            var emailFrom = System.Configuration.ConfigurationManager.AppSettings["emailFrom"];
            var emailTo = System.Configuration.ConfigurationManager.AppSettings["emailTo"];
            var emailHost = System.Configuration.ConfigurationManager.AppSettings["emailHost"];

            SmtpClient client = new SmtpClient();
            client.Host = emailHost;
            client.EnableSsl = false;
            client.UseDefaultCredentials = true;
            MailMessage mail = new MailMessage();
            mail.Sender = new MailAddress(emailFrom, "");
            mail.From = new MailAddress(emailFrom, "");
            mail.To.Add(new MailAddress(emailTo, ""));
            mail.Subject = "Exception no Sistema de Arrecadação";

            mail.Body = "Host: " + client.Host + "<br /><br />";
            mail.Body += "Mensagem: " + msg + "<br /><br />";
            mail.Body += "Exception: " + getType + "<br /><br />";
            mail.Body += "Data: " + data + "<br /><br />";
            mail.Body += "Source: " + source + "<br /><br />";
            mail.Body += "TargetSite: " + targetSite + "<br /><br />";
            mail.Body += "StackTrace: " + stackTrace + "<br /><br />";
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, exception.SystemExceptions(e));
            }
        }
    }
}
