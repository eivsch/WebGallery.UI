using System;
using System.Net.Http;
using Application.Services;
using Application.Services.Interfaces;
using AutoMapper;
using DomainModel.Aggregates.Gallery.Interfaces;
using DomainModel.Aggregates.Metadata.Interfaces;
using DomainModel.Aggregates.Picture.Interfaces;
using DomainModel.Aggregates.Tags.Interfaces;
using Infrastructure.Common;
using Infrastructure.Galleries;
using Infrastructure.Metadata;
using Infrastructure.Pictures;
using Infrastructure.Tags;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

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

            services.AddControllersWithViews();

            services.AddHttpClient<WebGalleryApiClient>(c => 
            {
                c.BaseAddress = new Uri(Configuration.GetValue("ConnectionStrings:ApiEndpoint", ""));
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient<WebGalleryFileServerClient>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {   
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            });
            
            services.AddTransient<IGalleryService, GalleryService>();
            services.AddTransient<IPictureService, PictureService>();
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<IMetadataService, MetadataService>();
            services.AddScoped<IFileService, FileService>();

            services.AddTransient<IGalleryRepository, GalleryRepository>();
            services.AddTransient<IPictureRepository, PictureRepository>();
            services.AddTransient<ITagRepository, TagRepository>();
            services.AddTransient<IMetadataRepository, MetadataRepository>();
            services.AddTransient<Infrastructure.Services.IFileSystemService, Infrastructure.Services.FileSystemService>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new Mappings.AutoMapperGalleryProfile());
                mc.AddProfile(new Mappings.AutoMapperPictureProfile());
                mc.AddProfile(new Mappings.AutoMapperTagProfile());
                mc.AddProfile(new Mappings.AutoMapperUploadProfile());
            });
            services.AddSingleton(mapperConfig.CreateMapper());

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
