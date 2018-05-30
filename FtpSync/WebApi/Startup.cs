using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using FtpSync.Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace FtpSync
{
    public class Startup
    {
        // Для сериализации js обьектов с маленькой буквы
        private void SetCamelCaseForAllSerialzedObjects(HttpConfiguration config)
        {
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.Formatting = Formatting.Indented;
        }


        public void Configuration(IAppBuilder app)
        {
            // In OWIN you create your own HttpConfiguration rather than
            // re-using the GlobalConfiguration.
            var config = new HttpConfiguration();

            SetCamelCaseForAllSerialzedObjects(config);

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "{controller}/{action}");

            var builder = new ContainerBuilder();

            // Register Web API controller in executing assembly.
            var dataAccess = Assembly.GetExecutingAssembly();
            builder.RegisterApiControllers(dataAccess);

            // OPTIONAL - Register the filter provider if you have custom filters that need DI.
            // Also hook the filters up to controllers.
            builder.RegisterWebApiFilterProvider(config);
            builder.RegisterType<CustomActionFilter>()
                .AsWebApiActionFilterFor<ChannelController>()
                .InstancePerRequest();

            builder.RegisterAssemblyTypes(dataAccess)
                .Where(t => t.Name.EndsWith("Rep"))
                .AsImplementedInterfaces();

            // Register a logger service to be used by the controller and middleware.
            //builder.Register(c => new Logger()).As<ILogger>().InstancePerRequest();

            // Autofac will add middleware to IAppBuilder in the order registered.
            // The middleware will execute in the order added to IAppBuilder.
            builder.RegisterType<FirstMiddleware>().InstancePerRequest();
            builder.RegisterType<SecondMiddleware>().InstancePerRequest();

            // Create and assign a dependency resolver for Web API to use.
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // The Autofac middleware should be the first middleware added to the IAppBuilder.
            // If you "UseAutofacMiddleware" then all of the middleware in the container
            // will be injected into the pipeline right after the Autofac lifetime scope
            // is created/injected.
            //
            // Alternatively, you can control when container-based
            // middleware is used by using "UseAutofacLifetimeScopeInjector" along with
            // "UseMiddlewareFromContainer". As long as the lifetime scope injector
            // comes first, everything is good.
            app.UseAutofacMiddleware(container);

            // Again, the alternative to "UseAutofacMiddleware" is something like this:
            app.UseAutofacLifetimeScopeInjector(container);
            //app.UseMiddlewareFromContainer<FirstMiddleware>();
            //app.UseMiddlewareFromContainer<SecondMiddleware>();

            // Make sure the Autofac lifetime scope is passed to Web API.
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }
    }
}
