using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
//Add services to the container (DI)

builder.Services.AddCarter(new DependencyContextAssemblyCatalog(typeof(Program).Assembly, Assembly.GetCallingAssembly()));
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

var app = builder.Build();

//configure the HTTP request pipeline

app.MapCarter();

app.Run();
