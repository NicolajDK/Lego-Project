using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Broen.Core.Helpers
{
    public static class UmbracoContextHelper
    {
        public static UmbracoContext GetAndEnsureUmbracoContext()
        {
            if (UmbracoContext.Current != null)
            {
                return UmbracoContext.Current;
            }

            var httpContext = new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));

            /* v7.3+ */
            return UmbracoContext.EnsureContext(
                httpContext,
                ApplicationContext.Current,
                new WebSecurity(httpContext, ApplicationContext.Current),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
        }

        public static void EnsureUmbracoContext()
        {
            if (UmbracoContext.Current == null)
            {
                var httpContext = new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));

                /* v7.3+ */
                UmbracoContext.EnsureContext(
                    httpContext,
                    ApplicationContext.Current,
                    new WebSecurity(httpContext, ApplicationContext.Current),
                    UmbracoConfig.For.UmbracoSettings(),
                    UrlProviderResolver.Current.Providers,
                    false);
            }
        }
    }
}
