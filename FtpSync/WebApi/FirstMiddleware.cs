using System.Threading.Tasks;
using Microsoft.Owin;
using NLog;

namespace FtpSync
{
    public class FirstMiddleware : OwinMiddleware
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public FirstMiddleware(OwinMiddleware next) : base(next)
        {
          
        }

        public override async Task Invoke(IOwinContext context)
        {

            await Next.Invoke(context);
        }
    }
}
