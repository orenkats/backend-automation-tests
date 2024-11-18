using Application.Messaging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register RabbitMQ services
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<RabbitMqConsumer>();

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RabbitMQ API", Version = "v1" });
});
Console.WriteLine("Application is starting...");
var app = builder.Build();

Console.WriteLine("Mapping routes...");
app.MapGet("/routes", (EndpointDataSource endpointDataSource) =>

{
    var routes = endpointDataSource.Endpoints.Select(e => e.DisplayName).ToList();
    return Results.Json(routes);
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.MapGet("/", () => "Welcome to the RabbitMQ Test API!");
var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
foreach (var endpoint in endpointDataSource.Endpoints)
{
    Console.WriteLine($"Endpoint: {endpoint.DisplayName}");
}
app.Run();
