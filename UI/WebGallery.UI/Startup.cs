using System;
using System.Net.Http;
using Infrastructure.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebGallery.UI.Configuration;

namespace WebGallery.UI
{
    public class Startup
    {
        public IWebHostEnvironment HostingEnvironment { get; }
        private bool IsDevelopmentEnv => HostingEnvironment?.EnvironmentName?.ToUpper() == "DEVELOPMENT";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
            // Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.Cookie.Name = "WebGallery.Cookies";
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/accessdenied";
                options.ExpireTimeSpan = TimeSpan.FromSeconds(3600);
                options.SlidingExpiration = true;
            });

            services.AddMemoryCache();
            services.AddControllersWithViews();
            services.Configure<DisplayOptions>(Configuration.GetSection("Display"));

            services.AddHttpClient<WebGalleryApiClient>(c => 
            {
                c.BaseAddress = new Uri(Configuration.GetValue("ConnectionStrings:ApiEndpoint", ""));
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.Timeout = TimeSpan.FromSeconds(600);
            });
            services.AddHttpClient<WebGalleryFileServerClient>(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(600);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {   
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            });

            services.AddTransient<Infrastructure.FileServer.IFileServerProxy, Infrastructure.FileServer.FileServerProxy>();
            services.AddTransient<Infrastructure.MinimalApi.MinimalApiProxy>();
            services.AddTransient<Infrastructure.Common.UsernameResolver>();

            if (!IsDevelopmentEnv)
                services.AddApplicationInsightsTelemetry();     // Should automatically get the key from configuration
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            // app.UseFileServer(new FileServerOptions
            // {
            //     FileProvider = new PhysicalFileProvider(Configuration.GetConnectionString("FileServerRoot")),
            //     RequestPath = "/files"
            // });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
