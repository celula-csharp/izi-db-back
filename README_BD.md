# SystemDB – Plataforma Multi-Motor de Base de Datos

Este documento describe el **modelo de datos principal (SystemDB)** usado por la plataforma.  
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

> Regla de negocio:  
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
