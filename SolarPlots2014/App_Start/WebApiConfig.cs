using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SolarPlots2014
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                //routeTemplate: "api/{controller}/latitude={latitude}&longitude={longitude}&timezone={timezone}&date={date}&increment={increment}&daylightsaving={daylightsaving}",
                //defaults: new { increment = RouteParameter.Optional, daylightsaving= RouteParameter.Optional }
                routeTemplate: "api/{controller}"

            );
        }
    }
}
