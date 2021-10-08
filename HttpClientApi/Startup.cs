using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using S3Client;
using System;
using System.Net.Http;

namespace HttpClientApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            ///--------------Note-------------------
            /// We implemented multi-tenant architecture 
            /// In this sample code we set all information in Environment Variable
            /// In actual case it will resolve/inject per request based on tenant
            ///--------------Note-------------------
            Environment.SetEnvironmentVariable("RegionEndpoint", configuration["RegionEndpoint"]);
            Environment.SetEnvironmentVariable("BucketName", configuration["BucketName"]);
            Environment.SetEnvironmentVariable("Prefix", configuration["Prefix"]);
            Environment.SetEnvironmentVariable("Key", configuration["Key"]);
            Environment.SetEnvironmentVariable("Secret", configuration["Secret"]);

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HttpClientApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HttpClientApi v1"));
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register<IDataProvider>(x =>
            {
                var clientFactory = x.Resolve<IHttpClientFactory>();
                IDataProvider dataProvider = new S3Blob(clientFactory);
                return dataProvider;
            }).InstancePerLifetimeScope();
        }
    }
}
