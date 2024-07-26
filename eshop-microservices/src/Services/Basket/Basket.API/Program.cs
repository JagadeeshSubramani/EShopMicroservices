using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container
var assembly = typeof(Program).Assembly;

builder.Services.AddCarter(new DependencyContextAssemblyCatalog(assembly, Assembly.GetCallingAssembly()));

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

var postgresDbConString = builder.Configuration.GetConnectionString("Database")!;

builder.Services.AddMarten(opts =>
{
    opts.Connection(postgresDbConString);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
//Decorator pattern setting
/*
builder.Services.AddScoped<IBasketRepository>(provider =>
{
    var basketRepository = provider.GetService<IBasketRepository>();
    return new CachedBasketRepository(basketRepository!, provider.GetService<IDistributedCache>()!);
});
*/
//Instead use Scrutor library
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
var redisConString = builder.Configuration.GetConnectionString("Redis")!;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConString;
    //options.InstanceName="Basket";
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresDbConString)
    .AddRedis(redisConString);

var app = builder.Build();

//Configure the HTTP pipeline

app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
