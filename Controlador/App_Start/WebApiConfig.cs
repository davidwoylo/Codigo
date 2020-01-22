using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Routing;

namespace CFC_Controlador
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            var cors = new EnableCorsAttribute("*", "*", "*");

            config.EnableCors(cors);

            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Filters.Add(new ExceptionHandler());

            //Web API routes
            config.MapHttpAttributeRoutes(new CustomDirectRouteProvider());

        }

        public class CustomDirectRouteProvider : DefaultDirectRouteProvider
        {
            protected override IReadOnlyList<IDirectRouteFactory>
            GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
            {
                return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>
                (inherit: true);
            }
        }

    }
}
