

using Serilog;
using ServiceCore.Middleware;
using ServiceOne.LoggingExtension;
using System.Reflection;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up logging");


try
{

    string _service_name;
    _service_name = Assembly.GetExecutingAssembly().GetName().Name;


    //var builder = WebApplication.CreateBuilder(args);

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
    //var configManager = new ConfigurationManager();
    //var settings = configManager.GetRequiredSection("Settings").Get<Settings>();

    //var settings = builder.Configuration.GetRequiredSection("Settings").Get<Settings>();

    //var connectionString = settings?.ConnectionString;


    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        //.AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
         .AddJsonFile($"appsettings.{environment}.json", true, true)
        .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
        //.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();
 
    //var services = new ServiceCollection()
    //    .AddOptions()
    //    .Configure<ApiSettings>(config.GetSection("ApiSettings"))
    //    .BuildServiceProvider();

    //var apiSettings = services.GetService<IOptions<ApiSettings>>();

    //Console.WriteLine(apiSettings.Value.ApiName);


    #endregion

    #region latihan untuk serilog
    //builder.Host.UseSerilog();

    //builder.Host.UseSerilog((hostContext, services, configuration) => {
    //    configuration.WriteTo.Console();
    //});

    //var logger = new LoggerConfiguration()
    //    .ReadFrom.Configuration(builder.Configuration)
    //    .Enrich.FromLogContext()
    //    .CreateLogger();

    //builder.Logging.ClearProviders();


    #endregion

    builder.Logging.ClearProviders().AddConsole().AddSerilog(CustomLoggerConfiguration.Configure());

    builder.Services.AddCors();
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
        app.UseSwaggerUI();
    }
    else
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpsRedirection();

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

    app.MapGet("/1", async context => await context.Response.WriteAsync(_service_name));
    //app.UseSwaggerUI(c =>
    //{
    //    string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
    //    c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "Web API");

    //});

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


