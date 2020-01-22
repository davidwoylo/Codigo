using AutoMapper;
using CFC_Negocio.Mapper;
using System;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;

namespace CFC_Controlador
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Mapper.Initialize(m => m.AddProfile<MappingProfile>());
            Mapper.AssertConfigurationIsValid();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
        }

        public override void Init()
        {
            this.PostAuthenticateRequest += MvcApplication_PostAuthenticateRequest;
            base.Init();
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            System.Web.HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }
    }
}