using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using NLog;

namespace FtpSync
{
    public class CustomActionFilter : IAutofacActionFilter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CustomActionFilter()
        {

        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {

            return Task.FromResult(0);
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {

            return Task.FromResult(0);
        }
    }
}
