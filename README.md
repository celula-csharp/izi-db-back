# Proyecto IZI-DB 

Este documento describe **todo lo que se implementó**, **qué necesitas instalado**, **cómo configurar el entorno**, **cómo ejecutar la API**, y **cómo probar cada motor de base de datos**.

---

# 1. Tecnologías utilizadas

El proyecto utiliza:

* **.NET 8 / .NET 7** (mínimo .NET 6)
* **C# ASP.NET Web API**
* **Patrón Factory** para conexiones a múltiples motores
* Motores soportados:

    * SQL Server
    * MySQL
    * PostgreSQL
    * MongoDB
    * Redis
* **xUnit** para pruebas unitarias

---

# 2. Requisitos previos

Asegúrate de tener instalado:

## ✔ .NET SDK

Descarga desde:
[https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download)

Verificar instalación:

```bash
dotnet --version
```

## ✔ Motores de base de datos

Debes tener instalados (local o Docker):

### SQL Server

* Local: SQL Server Developer Edition
* Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Admin123*" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### MySQL

```bash
docker run --name mysql -e MYSQL_ROOT_PASSWORD=admin -p 3306:3306 -d mysql:8
```

### PostgreSQL

```bash
docker run --name postgres -e POSTGRES_PASSWORD=admin -p 5432:5432 -d postgres
```

### MongoDB

```bash
docker run --name mongo -p 27017:27017 -d mongo
```

### Redis

```bash
docker run --name redis -p 6379:6379 -d redis
```

---

# 3. Configuración del archivo `appsettings.json`

Debe contener las siguientes conexiones:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;Database=master;User Id=sa;Password=Admin123*;TrustServerCertificate=True;",
    "MySql": "Server=localhost;Port=3306;Database=mysql;User=root;Password=admin;",
    "Postgres": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=admin;",
    "Mongo": "mongodb://localhost:27017",
    "Redis": "localhost:6379"
  }
}
```

---

# 4. Arquitectura implementada

Tu implementación tiene:

### ✔ Interfaz general de base de datos:

`IDatabaseConnection`

### ✔ Implementaciones concretas:

* `SqlServerConnection`
* `MySqlConnection`
* `PostgresConnection`
* `MongoConnection`
* `RedisConnection`

### ✔ Factory:

`DatabaseFactory` — devuelve la conexión correcta según el string del motor.

### ✔ Controlador de tests:

`TestController` con endpoints:

| Motor      | Endpoint             |
| ---------- | -------------------- |
| SQL Server | `/api/test/sql`      |
| MySQL      | `/api/test/mysql`    |
| PostgreSQL | `/api/test/postgres` |
| MongoDB    | `/api/test/mongo`    |
| Redis      | `/api/test/redis`    |

Cada endpoint:

* Obtiene cadena de conexión
* Llama a la factory
* Abre conexión
* Ejecuta instrucción de prueba real
* Devuelve JSON

---

# 5. Pruebas unitarias (xUnit)

Incluye pruebas para verificar:

* Que la factory devuelve el tipo correcto
* Que lanza excepción con motores inválidos
* Que cada motor ejecuta un comando básico
* Que abrir/cerrar conexión funciona

Ejecutar tests:

```bash
dotnet test
```

Si ves:

```
Total: 7 - Error: 0 - OK: 7
```

Entonces **todo está funcionando correctamente**.

---

# 6. Ejecutar la API

Desde la carpeta del proyecto:

```bash
dotnet run
```

Normalmente se abre en:

```
http://localhost:5267 (HTTPS)
```

---

# 7. Probar cada motor desde el navegador o Postman

Una vez corriendo, prueba:

### SQL Server

`GET http://localhost:5000/api/test/sql`

### MySQL

`GET http://localhost:5000/api/test/mysql`

### PostgreSQL

`GET http://localhost:5000/api/test/postgres`

### MongoDB

`GET http://localhost:5000/api/test/mongo`

### Redis

`GET http://localhost:5000/api/test/redis`

Si todos devuelven **200 OK** con un JSON como:

```json
{
  "engine": "MySql",
  "result": [ { "TestValue": 1 } ]
}
```

Entonces **la demostración real funciona**.

---

# ✔ 8. Checklist final dia 1

| Requisito                                            | Estado |
| ---------------------------------------------------- | ------ |
| Patrón Factory implementado                          | ✔      |
| Motores: SQL Server, MySQL, Postgres, MongoDB, Redis | ✔      |
| Pruebas unitarias xUnit                              | ✔      |
| TestController usando Factory                        | ✔      |
| Endpoints reales con queries de verdad               | ✔      |
| API funcionando sin errores                          | ✔      |
| Documentación README creada                          | ✔      |

---

# Completado dia 1

* Factory multipropósito
* Implementaciones reales
* Pruebas completas
* Controlador demostrativo
* Checklist y documentación

---