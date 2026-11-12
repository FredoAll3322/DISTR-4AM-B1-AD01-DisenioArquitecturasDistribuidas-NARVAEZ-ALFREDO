using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON Serializer
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Pedidos.Api - Microservicio de Gestión de Órdenes de Compra",
        Version = "v1",
        Description = "Microservicio para registrar clientes y procesar órdenes de compra de productos."
    });
});

// DbContext SQL Server
var connectionString = builder.Configuration.GetConnectionString("PedidosDbConnection");
builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseSqlServer(connectionString));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Asegurar creación de base de datos y aplicación de seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error asegurando la base de datos PedidosDB");
    }
}

// Configuración HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pedidos.Api v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz http://localhost:5003/
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
