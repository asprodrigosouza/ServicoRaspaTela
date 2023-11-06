using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspaTela.Dados.Extensions
{
    public static class ServiceBaseLiveTimeHostExtension
    {
        public static IHostBuilder UseServiceBaseLifeTime(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((hostContext,
            services) => services.AddSingleton<IHostLifetime,
            ServiceBaseLiveTime>());
        }

        public static Task RunAsServiceAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default) 
        {  
            return hostBuilder.UseServiceBaseLifeTime().Build().RunAsync(cancellationToken); 
        }
    }
}
