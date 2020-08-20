using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SampleWebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ViewReportHttpClient",
                url: "ViewReportHttpClient/{ReportName}/{SampleParameter}",
                defaults: new { controller = "Report", action = "HttpClientMode", ReportName = UrlParameter.Optional, SampleParameter = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "ViewReportJavaScript",
                url: "ViewReportJavaScript/{ReportName}/{SampleParameter}",
                defaults: new { controller = "Report", action = "JavaScriptMode", ReportName = UrlParameter.Optional, SampleParameter = UrlParameter.Optional }
            );
        }
    }
}
