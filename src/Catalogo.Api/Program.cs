using Microsoft.EntityFrameworkCore;
using Catalogo.Api.Data;

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
        Title = "Catalogo.Api - Microservicio de Catálogo Comercial",
        Version = "v1",
        Description = "Microservicio para administrar categorías, marcas y productos tecnológicos."
    });
});

// DbContext SQL Server
var connectionString = builder.Configuration.GetConnectionString("CatalogoDbConnection");
builder.Services.AddDbContext<CatalogoDbContext>(options =>
    options.UseSqlServer(connectionString));

// CORS (para permitir consumo desde Postman / frontends)
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
    var context = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error asegurando la base de datos CatalogoDB");
    }
}

// Configuración HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalogo.Api v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz http://localhost:5001/
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
