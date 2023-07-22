
using Serilog;
using System.Reflection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using ServiceGateway.StartupExtension;
using ServiceCore.Middleware;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.Provider.Eureka;
using System.Configuration;
using Steeltoe.Discovery.Client;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up logging");


try
{
    string _service_name;
    _service_name = Assembly.GetExecutingAssembly().GetName().Name;




    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ApplicationName = typeof(Program).Assembly.FullName,
        ContentRootPath = Directory.GetCurrentDirectory(),
        //EnvironmentName = Environments.Staging,
        //WebRootPath = "customwwwroot"
    });

    Console.WriteLine($"Application Name: {builder.Environment.ApplicationName}");
    Console.WriteLine($"Environment Name: {builder.Environment.EnvironmentName}");
    Console.WriteLine($"ContentRoot Path: {builder.Environment.ContentRootPath}");
    Console.WriteLine($"WebRootPath: {builder.Environment.WebRootPath}");


    // Add services to the container.

    #region belajar configuration
    
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        //.AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", true, true)
        .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
      //.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
       .AddJsonFile("ocelot.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"ocelot.{environment}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();


    #endregion

    builder.Logging.ClearProviders().AddConsole().AddSerilog(CustomLoggerConfiguration.Configure());

    builder.Services.AddCors();

    //builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

    builder.Services.AddOcelot(config)
        .AddEureka()
        .AddPolly()
        .AddCacheManager(x =>
        {
            x.WithDictionaryHandle();
        });

    builder.Services.AddDiscoveryClient(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddRouting(options => options.LowercaseUrls = true);

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        //app.UseSwaggerUI();
    }
    else
    {
        app.UseSwagger();
        //app.UseSwaggerUI();
    }

    //app.UseMiddleware<ExceptionMiddleware>();

    app.UseExceptionHandler(a => a.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature.Error;
        Log.Error(exception.Message);
        //logger.LogError(exception.Message);
        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }));
    
    app.UseDiscoveryClient();

    app.UseHttpsRedirection();

    app.UseCors(b => b
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
    app.UseStaticFiles();

    app.UseAuthorization();

   

   


    app.MapControllers();

    //dibwh ini supaya lgs meluncur ke swagger ketika production.
    app.MapGet("", context =>

        Task.Run(() =>
        {
            context.Response.Redirect("./swagger/index.html", permanent: false);
            return Task.FromResult(0);
        })

    );
    app.MapGet("/1", async context =>
        await context.Response.WriteAsync(_service_name));

    app.MapGet("/info", async context =>
        await context.Response.WriteAsync($"{_service_name}, running on {context.Request.Host}"));

    app.UseOcelot().Wait();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);

    //Log.Fatal(ex, "Unhandled Exception");
}

finally
{
    Log.Information("Log Complete");
    Log.CloseAndFlush();
}

