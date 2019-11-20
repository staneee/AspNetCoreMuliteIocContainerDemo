using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Common;

namespace WithAutofac
{
    public static class AbpAutofacHostBuilderExtensions
    {
        public static IHostBuilder UseAutofac(this IHostBuilder hostBuilder)
        {
            var containerBuilder = new ContainerBuilder();

            return hostBuilder.ConfigureServices((_, services) =>
            {
                services.AddObjectAccessor(containerBuilder);
            }).UseServiceProviderFactory(new AbpAutofacServiceProviderFactory(containerBuilder));
        }
    }
}
