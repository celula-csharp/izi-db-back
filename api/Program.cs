using infrastructure;
using application;
using System.Text;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Application.Auth.Services;
using Infrastructure.Auth;
using api.Swagger;
using application.Services;
using domain.Interfaces;
using infrastructure.Factory;
using AppPermissionService = application.Interfaces.IPermissionService;
using Application.Instances.Services;
using InstanceAssignmentService = application.Instances.InstanceAssignmentService;

var builder = WebApplication.CreateBuilder(args);

// DEPENDENCIES

// Factory de motores
builder.Services.AddSingleton<IDatabaseFactory, DatabaseFactory>();

// Servicio del esquema
builder.Services.AddScoped<ISchemaService, SchemaService>();

// Servicio de permisos
builder.Services.AddScoped<AppPermissionService, PermissionService>();

builder.Services.AddEndpointsApiExplorer();

// 1️⃣ Configurar DbContext MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SystemDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Inyectar servicios
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInstanceAssignmentService, InstanceAssignmentService>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllers();

// SWAGGER CONFIGURATION (SINGLE CONFIGURATION)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "IZI DB API", 
        Version = "v1",
        Description = "Plataforma multi-motor de base de datos - Core & Auth" 
    });
    
    // Custom document filter
    c.DocumentFilter<RoleBasedDocumentFilter>();

    // JWT Bearer definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el prefijo **Bearer**. Ejemplo: `Bearer eyJhbGci...`"
    });

    // Security requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// 2️⃣ Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSuperSecretaParaDesarrollo123!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "izi-db-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "izi-db-clients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero
    };
});

// 3️⃣ Authorization by roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StudentPolicy", policy => policy.RequireRole("Student"));
});

// CORS (Vital para que React funcione)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// 5️⃣ Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IZI DB API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
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

app.MapControllers();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ENDPOINTS

// Listado de motores
app.MapGet("/api/motores", () =>
{
    return new[] { "sqlserver", "mysql", "postgresql", "mongodb", "redis" };
})
.WithTags("Motores")
.RequireAuthorization("AdminPolicy");

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
.RequireAuthorization("AdminPolicy");

app.MapGet("/api/config", () => configs)
    .WithTags("Configuraciones")
    .RequireAuthorization("AdminPolicy");

// ENDPOINT DE SCHEMA
app.MapGet("/api/schema", async (
    string engine,
    string connectionString,
    ISchemaService schemaService) =>
{
    try
    {
        var result = await schemaService.GetSchemaAsync(engine, connectionString);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error al obtener schema: {ex.Message}");
    }
})
.WithTags("Schema")
.RequireAuthorization("AdminPolicy");

app.Run();