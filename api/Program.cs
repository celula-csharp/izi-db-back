using System.Text;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Application.Auth.Services;
using Infrastructure.Auth;
using Application.Instances.Services;
using Infrastructure.Instances;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configurar DbContext MySQL
var connectionString = builder.Configuration.GetConnectionString("SystemDB");
builder.Services.AddDbContext<SystemDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Inyectar servicios
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInstanceAssignmentService, InstanceAssignmentService>(); // <-- Esto es de Emmanuel

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

// 4️⃣ Controllers + CORS + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IZI DB API",
        Version = "v1",
        Description = "Plataforma multi-motor de base de datos - Core & Auth"
    });

    // Definición de Seguridad
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT."
    });

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

var app = builder.Build();

// 5️⃣ Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IZI DB API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
