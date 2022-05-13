using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace FChatSharp.ExamplePlugin
{
    public class ExamplePluginHostedService : IHostedService
    {
        private readonly IAbpApplicationWithExternalServiceProvider _application;
        private readonly IServiceProvider _serviceProvider;
        private readonly ExamplePlugin _examplePlugin;

        public ExamplePluginHostedService(
            IAbpApplicationWithExternalServiceProvider application,
            IServiceProvider serviceProvider,
            ExamplePlugin vcBot)
        {
            _application = application;
            _serviceProvider = serviceProvider;
            _examplePlugin = vcBot;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _application.Initialize(_serviceProvider);
            _examplePlugin.Run();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();
            return Task.CompletedTask;
        }
    }
}
