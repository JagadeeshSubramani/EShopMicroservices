using Microsoft.Extensions.Caching.Distributed;
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

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
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
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    //options.InstanceName="Basket";
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

//Configure the HTTP pipeline

app.MapCarter();

app.UseExceptionHandler(options => { });

app.Run();
