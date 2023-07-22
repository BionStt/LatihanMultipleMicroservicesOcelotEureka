

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace ServiceOne.LoggingExtension
{
    public class CustomLoggerConfiguration
    {
        public static Logger Configure()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", true, true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                //.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                //.Enrich.WithProperty("ApplicationName", environment.)
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithCorrelationId()
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .WriteTo.Console()
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                //.WriteTo.Elasticsearch(ConfigureElasticSearchSink())
                
                .ReadFrom.Configuration(configuration)
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();
        }
        static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                //IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
                IndexFormat = $"logging-microservice-architecture-demo-{DateTime.UtcNow:yyyy-MM}",
                //MinimumLogEventLevel = LogEventLevel.Debug

            };
        }
    }

   
}
