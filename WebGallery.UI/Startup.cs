using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Interfaces;
using AutoMapper;
using DomainModel.Aggregates.Gallery.Interfaces;
using DomainModel.Aggregates.Picture.Interfaces;
using DomainModel.Aggregates.Tags.Interfaces;
using Infrastructure.Common;
using Infrastructure.Galleries;
using Infrastructure.Pictures;
using Infrastructure.Tags;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebGallery.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddHttpClient<ApiClient>(c => 
            {
                c.BaseAddress = new Uri(Configuration.GetValue("ConnectionStrings:ApiEndpoint", ""));
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            
            services.AddTransient<IGalleryRepository, GalleryRepository>();
            services.AddTransient<IGalleryService, GalleryService>();
            services.AddTransient<ITagService, TagService>();

            services.AddTransient<IPictureService, PictureService>();
            services.AddTransient<IPictureRepository, PictureRepository>();
            services.AddTransient<ITagRepository, TagRepository>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new Mappings.AutoMapperGalleryProfile());
                mc.AddProfile(new Mappings.AutoMapperPictureProfile());
                mc.AddProfile(new Mappings.AutoMapperTagProfile());
            });
            services.AddSingleton(mapperConfig.CreateMapper());
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

            app.UseRouting();

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
