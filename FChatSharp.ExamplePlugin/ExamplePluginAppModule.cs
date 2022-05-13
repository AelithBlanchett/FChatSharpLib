using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace FChatSharp.ExamplePlugin
{

    [DependsOn(
        typeof(AbpAutofacModule)
    )]
    public class ExamplePluginAppModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddHostedService<ExamplePluginHostedService>();
        }

        public const string RemoteServiceName = "Default";
    }
}
