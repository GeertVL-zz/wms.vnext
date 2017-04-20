using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using OrderingAPI.Infrastructure.Filters;
using Microsoft.Extensions.HealthChecks;
using Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using System.Data.Common;
using Wms.BuildingBlocks.IntegrationEventLogEF.Services;
using OrderingAPI.IntegrationEvents;
using Wms.BuildingBlocks.EventBus.Abstractions;
using Wms.BuildingBlocks.EventBusRabbitMQ;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace OrderingAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets(typeof(Startup).GetTypeInfo().Assembly);
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            }).AddControllersAsServices();

            services.AddHealthChecks(checks =>
            {
                checks.AddSqlCheck("OrderingDb", Configuration["ConnectionString"]);
            });

            services.AddEntityFrameworkSqlServer()
                .AddDbContext<OrderingContext>(options =>
                {
                    options.UseSqlServer(Configuration["ConnectionString"],
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                }, ServiceLifetime.Scoped);
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                options.DescribeAllEnumsAsStrings();
                options.SingleApiVersion(new Swashbuckle.Swagger.Model.Info()
                {
                    Title = "Ordering HTTP Api",
                    Version = "v1",
                    Description = "The Ordering Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(sp => (DbConnection c) => new IntegrationEventLogService(c));
            var serviceProvider = services.BuildServiceProvider();
            services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();
            services.AddSingleton<IEventBus>(new EventBusRabbitMQ(Configuration["EventBusConnection"]));
            services.AddOptions();

            var container = new ContainerBuilder();
            container.Populate(services);

            container.RegisterModule(new MediatorModule());
            container.RegisterModule(new ApplicationModule(Configuration["ConnectionString"]));

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
