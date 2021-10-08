using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HttpClientApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((hostingContext, config) =>
           {
               config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile("configoverride/appsettings.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables();
           }).UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
