using api.Swagger;
using application.Services;
using domain.Interfaces;
using infrastructure.Factory;
using Microsoft.OpenApi.Models;
using AppPermissionService = application.Interfaces.IPermissionService;

var builder = WebApplication.CreateBuilder(args);

//DEPENDENCIAS

// Factory de motores
builder.Services.AddSingleton<IDatabaseFactory, DatabaseFactory>();

// Servicio del esquema
builder.Services.AddScoped<ISchemaService, SchemaService>();

// Servicio de permisos
builder.Services.AddScoped<AppPermissionService, PermissionService>();

builder.Services.AddEndpointsApiExplorer();

//SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IZI Database API", Version = "v1" });
    c.DocumentFilter<RoleBasedDocumentFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Token simulado. EJ: Bearer admin",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware: simula usuario autenticado por header
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"]
        .ToString()
        ?.Replace("Bearer ", "");

    if (!string.IsNullOrWhiteSpace(token))
    {
        context.Items["UserId"] = "123";
        context.Items["Role"] = token.ToLower(); // admin/user
    }

    await next();
});

//ENDPOINTS

// Listado de motores
app.MapGet("/api/motores", () =>
{
    return new[] { "sqlserver", "mysql", "postgresql", "mongodb", "redis" };
})
.WithTags("Motores")
.RequireAuthorization("admin");

// Ejecutar consultas
app.MapPost("/api/query", async (
    HttpContext context,
    string engine,
    string connectionString,
    string query,
    IDatabaseFactory factory) =>
{
    var role = context.Items["Role"]?.ToString();
    
    if (role == "user")
    {
        var allowedEngine = "postgresql";
        var allowedConnection = "Server=localhost;Port=5432;User Id=customer;Password=1234;Database=mi_bd;";

        if (engine.ToLower() != allowedEngine.ToLower())
            return Results.Forbid();

        if (connectionString.Trim() != allowedConnection.Trim())
            return Results.Forbid();
    }
    var db = factory.Create(engine, connectionString);

    if (db == null)
        return Results.BadRequest("Motor no soportado.");

    try
    {
        await db.Open();
        var result = await db.ExecuteQuery(query);
        await db.Close();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithTags("Consultas");

// Configs internas (demo)
var configs = new List<object>();

app.MapPost("/api/config", (object config) =>
{
    configs.Add(config);
    return Results.Ok(config);
})
.WithTags("Configuraciones")
.RequireAuthorization("admin");

app.MapGet("/api/config", () => configs)
    .WithTags("Configuraciones")
    .RequireAuthorization("admin");

//ENDPOINT DE SCHEMA

app.MapGet("/api/schema", async (
    string engine,
    string connectionString,
    ISchemaService schemaService) =>
{
    var result = await schemaService.GetSchemaAsync(engine, connectionString);

    return result == null
        ? Results.BadRequest("Motor no soportado o no tiene schema.")
        : Results.Ok(result);
})
.WithTags("Schema")
.RequireAuthorization("admin");

app.Run();
