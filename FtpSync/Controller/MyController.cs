using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;

namespace FtpSync.Controller
{
    public class MyController : ApiController
    {
        internal readonly DataContext db = new DataContext();
        internal static readonly Logger logger = LogManager.GetCurrentClassLogger();
    }
}
