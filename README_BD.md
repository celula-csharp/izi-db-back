# SystemDB – Plataforma Multi-Motor de Base de Datos

Este documento describe el **modelo de datos principal (SystemDB)** y su configuración en **EF Core** para la plataforma _IZI DB_.

El objetivo de esta base de datos es **gestionar la identidad de los usuarios, sus roles y la asignación de instancias de base de datos**.

---

## 1. Entidades del Dominio

### 1.1. Role

Representa el rol que puede tener un usuario en el sistema (por ejemplo: `Admin`, `Student`).

**Tabla:** `Roles`

**Campos:**

- `Id` (int, PK)
- `Name` (string, requerido, único, máx. 50)
- `Description` (string, opcional)

**Relaciones:**

- `Role 1 - N User`  
  Un rol puede estar asignado a muchos usuarios.

---

### 1.2. User

Representa a un usuario autenticable dentro de la plataforma.

**Tabla:** `Users`

**Campos:**

- `Id` (int, PK)
- `Username` (string, requerido, único, máx. 100)
- `Email` (string, requerido, único, máx. 150)
- `PasswordHash` (string, requerido)
- `IsActive` (bool, por defecto `true`)
- `CreatedAt` (datetime, por defecto `UtcNow`)
- `RoleId` (int, FK a `Roles.Id`)

**Relaciones:**

- `User N - 1 Role`
- `User 1 - 1 UserInstance`  
  Un usuario puede tener como máximo **una instancia asignada**.

---

### 1.3. DatabaseInstance

Representa una instancia de base de datos disponible en la plataforma (MySQL, PostgreSQL, etc).

**Tabla:** `DatabaseInstances`

**Campos:**

- `Id` (int, PK)
- `Name` (string, requerido, máx. 100)
- `Description` (string, opcional, máx. 250)
- `ConnectionString` (string, opcional, máx. 500)
- `IsActive` (bool, por defecto `true`)

**Relaciones:**

- `DatabaseInstance 1 - N UserInstance`

---

### 1.4. UserInstance

Representa la **asignación de una instancia** a un usuario (especialmente estudiantes).

**Tabla:** `UserInstances`

**Campos:**

- `Id` (int, PK)
- `UserId` (int, FK a `Users.Id`)
- `DatabaseInstanceId` (int, FK a `DatabaseInstances.Id`)
- `AssignedAt` (datetime, por defecto `UtcNow`)

**Relaciones:**

- `UserInstance N - 1 DatabaseInstance`
- `UserInstance 1 - 1 User` (índice único en `UserId`)

> 🔐 **Regla de negocio:**  
> **Un estudiante solo puede tener 1 instancia asignada.**  
> Esto se garantiza con un índice `UNIQUE(UserId)` en la tabla `UserInstances`.

---

## 2. Diagrama ER (Mermaid)

El diagrama ER está documentado en:

- `docs/diagrams/systemdb.mmd`

Diagrama en Mermaid:

```mermaid
erDiagram
    ROLE ||--o{ USER : "has many"
    USER ||--|| USERINSTANCE : "has one"
    DATABASEINSTANCE ||--o{ USERINSTANCE : "assigned to"

    ROLE {
        int Id PK
        string Name
        string Description
    }

    USER {
        int Id PK
        string Username
        string Email
        string PasswordHash
        bool IsActive
        datetime CreatedAt
        int RoleId FK
    }

    DATABASEINSTANCE {
        int Id PK
        string Name
        string Description
        string ConnectionString
        bool IsActive
    }

    USERINSTANCE {
        int Id PK
        int UserId FK
        int DatabaseInstanceId FK
        datetime AssignedAt
    }
3. Configuración de EF Core
3.1. DbContext
El contexto principal del sistema se llama SystemDbContext y vive en:

infrastructure/Data/SystemDbContext.cs

Este contexto expone los DbSet:

csharp
Copiar código
public class SystemDbContext : DbContext
{
    public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options) { }

    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<DatabaseInstance> DatabaseInstances { get; set; } = null!;
    public DbSet<UserInstance> UserInstances { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role 1 - N User
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Users)
            .WithOne(u => u.Role!)
            .HasForeignKey(u => u.RoleId);

        // User 1 - 1 UserInstance (un estudiante = 1 instancia)
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserInstance)
            .WithOne(ui => ui.User)
            .HasForeignKey<UserInstance>(ui => ui.UserId);

        modelBuilder.Entity<UserInstance>()
            .HasIndex(ui => ui.UserId)
            .IsUnique();

        // DatabaseInstance 1 - N UserInstance
        modelBuilder.Entity<DatabaseInstance>()
            .HasMany(di => di.UserInstances)
            .WithOne(ui => ui.DatabaseInstance)
            .HasForeignKey(ui => ui.DatabaseInstanceId);
    }
}
3.2. Conexión MySQL en Program.cs (API)
En el proyecto api, el DbContext se registra así:

csharp
Copiar código
var connectionString = builder.Configuration.GetConnectionString("SystemDB");

builder.Services.AddDbContext<SystemDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
Cadena de conexión configurada (ejemplo):

json
Copiar código
"ConnectionStrings": {
  "SystemDB": "server=csharp-database-csharp.g.aivencloud.com;database=izi;port=22194;user=avnadmin;password=ABc**ddf**c"
}
3.3. Paquetes NuGet usados para la capa de datos
En el proyecto infrastructure se utilizan:

Microsoft.EntityFrameworkCore

Microsoft.EntityFrameworkCore.Relational

Pomelo.EntityFrameworkCore.MySql

4. Migraciones EF Core (YA CREADAS)
Para la base de datos SystemDB ya se creó y aplicó la migración inicial.

Comandos usados:

bash
Copiar código
# Crear migración inicial
dotnet ef migrations add InitialSystemDbMigration -p infrastructure -s api

# Aplicar migraciones a la base de datos MySQL configurada en SystemDB
dotnet ef database update -p infrastructure -s api
La migración genera las tablas:

Roles

Users

DatabaseInstances

UserInstances

incluyendo:

FK entre Users.RoleId → Roles.Id

FK entre UserInstances.UserId → Users.Id (con índice único)

FK entre UserInstances.DatabaseInstanceId → DatabaseInstances.Id