using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;
using ProductAPI.InfraStructure.Filters;
using ProductAPI.InfraStructure;
using Microsoft.Extensions.HealthChecks;
using System.Data.Common;
using Wms.BuildingBlocks.IntegrationEventLogEF.Services;
using Microsoft.Extensions.Options;
using ProductAPI.IntegrationEvents;
using Wms.BuildingBlocks.EventBus.Abstractions;
using Wms.BuildingBlocks.EventBusRabbitMQ;
using Wms.BuildingBlocks.IntegrationEventLogEF;
using System.Data.SqlClient;

namespace ProductAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

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
            services.AddHealthChecks(checks =>
            {
                checks.AddSqlCheck("ProductDb", Configuration["ConnectionString"]);
            });

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            }).AddControllersAsServices();

            services.AddDbContext<ProductContext>(options =>
            {
                options.UseSqlServer(Configuration["ConnectionString"],
                               sqlServerOptionsAction: sqlOptions =>
                               {
                                       sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                       sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                   });
                options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            });

            services.Configure<Settings>(Configuration);

            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SingleApiVersion(new Swashbuckle.Swagger.Model.Info()
                {
                    Title = "wms.vnext - Product HTTP API",
                    Version = "v1",
                    Description = "The Product Microservice HTTP API"
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

            services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(sp => (DbConnection c) => new IntegrationEventLogService(c));
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IOptionsSnapshot<Settings>>().Value;
            services.AddTransient<IProductIntegrationEventService, ProductIntegrationEventService>();
            services.AddSingleton<IEventBus>(new EventBusRabbitMQ(configuration.EventBusConnection));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("CorsPolicy");

            app.UseMvcWithDefaultRoute();

            app.UseSwagger().UseSwaggerUi();

            var context = (ProductContext)app.ApplicationServices.GetService(typeof(ProductContext));

            WaitForSqlAvailability(context, loggerFactory);

            ProductContextSeed.SeedAsync(app, loggerFactory).Wait();

            var integrationEventLogContext = new IntegrationEventLogContext(
                new DbContextOptionsBuilder<IntegrationEventLogContext>()
                .UseSqlServer(Configuration["ConnectionString"], b => b.MigrationsAssembly("Product.API"))
                .Options);
            integrationEventLogContext.Database.Migrate();
        }

        private void WaitForSqlAvailability(ProductContext ctx, ILoggerFactory loggerFactory, int? retry = 0)
        {
            int retryForAvailability = retry.Value;
            try
            {
                ctx.Database.OpenConnection();
            }
            catch (SqlException ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;
                    var log = loggerFactory.CreateLogger(nameof(Startup));
                    log.LogError(ex.Message);
                    WaitForSqlAvailability(ctx, loggerFactory, retryForAvailability);
                }
            }
            finally
            {
                ctx.Database.CloseConnection();
            }
        }
    }
}