using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Interface;
using FtpSync.Real;
using Ninject;

namespace FtpSync
{
    static class Inject
    {
        public static readonly StandardKernel Conteiner = new StandardKernel();

        public static void SetDependenciesTest()
        {
            // Получение данных о видеорегистраторах
            Conteiner.Bind<IVideoRegRepository>().To<TestVideoRegRepositoty>();
        }

        public static void SetDependencies()
        {
            // Получение данных о видеорегистраторах
        }
    }
}
