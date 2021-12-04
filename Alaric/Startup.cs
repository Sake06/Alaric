using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Alaric.Shared;
using Alaric.DB;
using Microsoft.EntityFrameworkCore;
using Alaric.DB.Models;
using Microsoft.OpenApi.Models;
using Alaric.Messaging.RabbitMq;
using Alaric.Messaging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Alaric
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

            services.AddControllers();
            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("TradeDB"));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trade API", Version = "v1" });
            });
           
            var serviceClientSettingsConfig = Configuration.GetSection("RabbitMq");
            var serviceClientSettings = serviceClientSettingsConfig.Get<RabbitMqConfiguration>();
            var serviceClientBaseConfig = Configuration.GetSection("BaseOptions");
            var BaseConfig = serviceClientBaseConfig.Get<BaseOptions>();
        
            services.Configure<RabbitMqConfiguration>(serviceClientSettingsConfig);
            services.Configure<BaseOptions>(serviceClientBaseConfig);

            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddHostedService<RabbitMqConsumer>();
            services.AddSingleton<IPublisher, RabbitmqPublisher>(); 
            services.AddSingleton<Orchestrator>();
            services.AddSingleton<Dispatcher>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            };

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger");
            });
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetService<IDbInitializer>(); 
                dbInitializer.SeedData();
            }
            var publisher = app.ApplicationServices.GetRequiredService<IPublisher>();
            publisher.Init();



            var opts = app.ApplicationServices.GetRequiredService<IOptions<BaseOptions>>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            logger.LogInformation("APPLICATION ID =" +  opts.Value.ApplicationId.ToString());

        }


    }
}
