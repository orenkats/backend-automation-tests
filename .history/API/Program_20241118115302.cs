using Application.Messaging;
using Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Register RabbitMQ services
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 29)) // Replace with your MySQL version
    ));
var app = builder.Build();

app.MapControllers();
app.MapGet("/", () => "Welcome to the RabbitMQ Test API!");
app.MapGet("/routes", (EndpointDataSource endpointDataSource) =>
{
    var routes = endpointDataSource.Endpoints.Select(e => e.DisplayName).ToList();
    return Results.Json(routes);
});

app.Run();
