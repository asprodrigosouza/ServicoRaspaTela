using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaspaTela.Dados.Extensions;
using RaspaTela.Dados.Models;
using RaspaTela.Servico;
using System.Diagnostics;

namespace RaspaTela.Dados
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var asService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<RaspaTelaService>();
                });

            builder.UseEnvironment(asService ? EnvironmentName.Production : EnvironmentName.Development);

            if (asService)
                await builder.RunAsServiceAsync();
            else
                await builder.RunConsoleAsync();            
        }
    }
}