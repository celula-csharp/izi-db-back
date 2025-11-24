# SystemDB – Multi-Engine Database Platform

This document describes the **main data model (SystemDB)** used by the platform.  
The goal of this database is to **manage user identity, roles, and database instance assignments**.

---

## 1. Domain Entities

### 1.1. Role

Represents the role a user can have in the system (for example: `Admin`, `Student`).

**Table:** `Roles`

**Fields:**

- `Id` (int, PK)
- `Name` (string, required, unique, max 50)
- `Description` (string, optional)

**Relationships:**

- `Role 1 - N User`  
  One role can be assigned to many users.

---

### 1.2. User

Represents an authenticable user in the platform.

**Table:** `Users`

**Fields:**

- `Id` (int, PK)
- `Username` (string, required, unique, max 100)
- `Email` (string, required, unique, max 150)
- `PasswordHash` (string, required)
- `IsActive` (bool, default `true`)
- `CreatedAt` (datetime, default `UtcNow`)
- `RoleId` (int, FK to `Roles.Id`)

**Relationships:**

- `User N - 1 Role`
- `User 1 - 1 UserInstance`  
  A user (especially with `Student` role) can have at most **one assigned instance**.

---

### 1.3. DatabaseInstance

Represents a database engine instance available in the platform (MySQL, PostgreSQL, etc).

**Table:** `DatabaseInstances`

**Fields:**

- `Id` (int, PK)
- `Name` (string, required, max 100)
- `Description` (string, optional, max 250)
- `ConnectionString` (string, optional, max 500)
- `IsActive` (bool, default `true`)

**Relationships:**

- `DatabaseInstance 1 - N UserInstance`  
  A single instance can be assigned to multiple students (depending on system rules).

---

### 1.4. UserInstance

Represents the **assignment of a database instance** to a user (typically students).

**Table:** `UserInstances`

**Fields:**

- `Id` (int, PK)
- `UserId` (int, FK to `Users.Id`)
- `DatabaseInstanceId` (int, FK to `DatabaseInstances.Id`)
- `AssignedAt` (datetime, default `UtcNow`)

**Relationships:**

- `UserInstance N - 1 DatabaseInstance`
- `UserInstance 1 - 1 User` (unique index on `UserId`)

> Business rule:  
> **A student can only have 1 instance assigned.**  
> This is enforced at business logic level and/or with a `UNIQUE(UserId)` index on the `UserInstances` table.

---

## 2. ER Diagram (Mermaid)

The ER diagram is documented in:

- `docs/diagrams/systemdb.mmd`

Mermaid diagram:

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

3. SystemDbContext Configuration (technical summary)

    Project: infrastructure
    File: Infrastructure/Data/SystemDbContext.cs

    The DbContext is configured with the following entities:

        DbSet<User>

        DbSet<Role>

        DbSet<DatabaseInstance>

        DbSet<UserInstance>

    Relationships are defined using Fluent API.

    A unique constraint on UserInstance.UserId can be added to enforce the “1 student = 1 instance” rule.

Conceptual example:

modelBuilder.Entity<User>()
    .HasOne(u => u.Role)
    .WithMany(r => r.Users)
    .HasForeignKey(u => u.RoleId);

modelBuilder.Entity<UserInstance>()
    .HasOne(ui => ui.User)
    .WithOne(u => u.UserInstance)
    .HasForeignKey<UserInstance>(ui => ui.UserId);

modelBuilder.Entity<UserInstance>()
    .HasOne(ui => ui.DatabaseInstance)
    .WithMany(di => di.UserInstances)
    .HasForeignKey(ui => ui.DatabaseInstanceId);

4. Migrations and Physical Database

    Note: This is executed by the team using EF Core and SystemDbContext.

General steps:

    Create the initial migration:

dotnet ef migrations add InitialSystemDbMigration -p infrastructure -s api

Apply the migration to the MySQL database:

    dotnet ef database update -p infrastructure -s api

    Verify in MySQL that these tables were created:

        Roles

        Users

        DatabaseInstances

        UserInstances

5. MySQL Connection

Example configuration (in appsettings.json or appsettings.Development.json):

"ConnectionStrings": {
  server=csharp-database-csharp.g.aivencloud.com;database=webEscuela_db;port=22194;user=avnadmin;password=AVNS_Us7LpZpO9OxWMsZo0w0
}

And in Program.cs:

var connectionString = builder.Configuration.GetConnectionString("SystemDB");

builder.Services.AddDbContext<SystemDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

6. Team & Responsibilities
Emmanuel – Core & Domain Model

    Designed the domain model:

        Entities User, Role, DatabaseInstance, UserInstance.

        Relations and business rules.

    Configured SystemDbContext (1:N and 1:1 relationships).

    Documented:

        Table and field structure.

        ER diagram in Mermaid (docs/diagrams/systemdb.mmd).

        Business rule: “1 student = 1 instance”.

Daniel – Migrations & Physical Database

    Created Entity Framework Core migrations.

    Applied migrations to the MySQL database (SystemDB).

    Verified the final schema in MySQL.

    Adjusted indexes, constraints, and performed initial connection tests.