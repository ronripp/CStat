using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CStat.Data;
using Microsoft.Extensions.DependencyInjection;
using CStat.Common;
using System.IO;

namespace CStat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("CStat.json",
                        optional: true,
                        reloadOnChange: true);
                })

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory())
                       .UseKestrel()
                       .UseIISIntegration()
                       .UseIIS()
                       .UseStartup<Startup>();
                });
            
                //.ConfigureServices(services => services.AddHostedService<CStatBkgService>());
    }
}
