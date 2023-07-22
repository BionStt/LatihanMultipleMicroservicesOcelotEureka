
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Newtonsoft.Json;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Polly;
using Serilog;
using ServiceGatewayWithOpenAPI.Config;
using ServiceGatewayWithOpenAPI.StartupExtension;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

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

    #region belajar configuration
    var routes = "Routes";
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    #if DEBUG
        routes = "Routes";
    #else
    routes = "Routes.prod";
    #endif
        ;

    //var config = new ConfigurationBuilder()
    //    .SetBasePath(Directory.GetCurrentDirectory())
    //    //.AddJsonFile("appsettings.json")
    //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //    .AddJsonFile($"appsettings.{environment}.json", true, true)
    //    .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
    //    //.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
    //    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: false)
    //    .AddJsonFile($"ocelot.{environment}.json", optional: true)
    //    .AddOcelotWithSwaggerSupport(options =>
    //    {
    //        options.Folder = routes;
    //    })
    //    //.AddOcelot(routes, builder.Environment)
    //    .AddEnvironmentVariables()
    //    .Build();
    
    
    // Register service discovery - Eureka
    builder.Services.AddServiceDiscovery(o => o.UseEureka());

    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", true, true)
        .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"ocelot.{environment}.json", optional: true)
        .AddOcelotWithSwaggerSupport(options =>
        {
            options.Folder = routes;
        })
        .AddOcelot(routes, builder.Environment)
        .AddEnvironmentVariables();

    #endregion

    builder.Services
        .AddOcelot(builder.Configuration)
        .AddEureka()
        .AddPolly()
        .AddCacheManager(x =>
        {
            x.WithDictionaryHandle();
        });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddRouting(options => options.LowercaseUrls = true);

    builder.Services.AddSwaggerForOcelot(builder.Configuration);

    builder.Logging.ClearProviders().AddConsole().AddSerilog(CustomLoggerConfiguration.Configure());

    builder.Services.AddCors();


    // Add services to the container.

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

    app.MapGet("/", async context =>
        await context.Response.WriteAsync(_service_name));

    app.MapGet("/info", async context =>
        await context.Response.WriteAsync($"{_service_name}, running on {context.Request.Host}"));

    app.UseSwaggerForOcelotUI(options =>
    {
        options.PathToSwaggerGenerator = "/swagger/docs";
        options.ReConfigureUpstreamSwaggerJson = AlterUpstream.AlterUpstreamSwaggerJson;

    }).UseOcelot().Wait();

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
