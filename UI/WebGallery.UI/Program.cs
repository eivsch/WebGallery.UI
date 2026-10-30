using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebGallery.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseUrls(
                            $"http://0.0.0.0:{Environment.GetEnvironmentVariable("WEBGALLERY_HTTP_PORT") ?? "5050"}",
                            $"https://0.0.0.0:{Environment.GetEnvironmentVariable("WEBGALLERY_HTTPS_PORT") ?? "5051"}")
                    ;
                });
    }
}
