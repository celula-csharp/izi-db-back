
---

## Proyecto Multi-DB Query Executor
Este proyecto permite conectarse y ejecutar consultas en SQL Server, MySQL, PostgreSQL, MongoDB y Redis, bajo una arquitectura DDD + Clean Architecture.

Incluye validaciones de permisos, ejecución segura y documentación de la sintaxis aceptada.

---

## **1. Requisitos Previos**
Asegúrate de tener instalado:

✔ .NET 8 SDK

Descargar desde: https://dotnet.microsoft.com/download

```bash
    dotnet --version
```

✔ PostgreSQL
```bash
    sudo apt install postgresql postgresql-client
```

✔ MySQL
```bash
    sudo apt install mysql-server
```

✔ SQL Server
```bash
    sudo apt-get install mssql-server
```

✔ MongoDB
```bash
    sudo apt install mongodb
```

✔ Redis
```bash
    sudo apt install redis
```

✔ GH CLI (opcional)
```bash
    sudo apt install gh
```

---

## **2. Configuración – appsettings.json**
Agrega tus conexiones:
```bash
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;Database=test;User Id=sa;Password=YourPassword;TrustServerCertificate=True;",
    "MySql": "Server=localhost;Database=test;User=root;Password=1234;",
    "Postgres": "Host=localhost;Database=test;Username=postgres;Password=1234;",
    "Mongo": "mongodb://localhost:27017",
    "Redis": "localhost:6379"
  }
}
```

---

## **3. Ejecutar el Proyecto**
Desde la raíz del proyecto:
```bash
  cd api
  dotnet run
```
La API corre por defecto en:
```bash
https://localhost:
```

---

## **4. Probar Conexiones**
La API expone endpoints para probar cada motor:
```bash
GET /api/test/sql
GET /api/test/mysql
GET /api/test/postgres
GET /api/test/mongo
GET /api/test/redis
```
Ejemplo:
```bash
curl https://localhost:7021/api/test/mysql
```

---

## **5. Endpoint Principal**
POST `/api/query/execute`
Body:
```json
{
  "instanceId": 12,
  "query": "SELECT * FROM products"
}
```
# ✔ Validaciones del Endpoint `/api/query/execute`

Este endpoint retorna distintos códigos HTTP dependiendo del resultado de la ejecución y las validaciones aplicadas.

##  Tabla de Validaciones

| Caso                       | Respuesta         |
|---------------------------|-------------------|
| Query correcta            | **200 OK**        |
| Query inválida            | **400 Bad Request** |
| No permisos               | **403 Forbidden** |
| Instancia no existe       | **404 Not Found** |

---

## **6. Servicios**
Los servicios creados:

✔ IInstanceService

Obtiene datos de la instancia.

✔ IPermissionService

Valida si el usuario puede acceder (Admin = acceso total).

✔ IQueryExecutor

Ejecuta consultas reales contra motores.

Todo está ya integrado en `QueryController`.

---

## **7. Cómo Probar el Endpoint Principal**
En Postman:

1️⃣ Crear Request

POST →
```bash
https://localhost:7021/api/query/execute
```
2️⃣ Headers
```pgsql
Authorization: Bearer {jwt}
Content-Type: application/json
```
3️⃣ Body
```json
{
  "instanceId": 1,
  "query": "SELECT 1"
}
```

---

## QUERY GUIDE – Syntax & Rules
Documentación inicial de la sintaxis aceptada para cada motor.

---

## **1. SQL Server (T-SQL Básico)**
✔ Comandos permitidos

`SELECT`, `INSERT`, `UPDATE`, `DELETE`

`WHERE`, `ORDER BY`, `GROUP BY`

Funciones: `COUNT`, `SUM`, `GETDATE()`

✔ Ejemplos
```bash
SELECT * FROM Products;
```
```bash
SELECT Name FROM Users WHERE Active = 1;
```

---

## 2. MySQL
✔ Comandos permitidos

`SELECT`, `UPDATE`, `DELETE`, `INSERT`

`WHERE`, `JOIN`, `LIMIT`

✔ Ejemplos
```bash
SELECT * FROM customers LIMIT 10;
```

---

## 3. PostgreSQL
✔ Comandos permitidos

`SELECT`, `INSERT`, `UPDATE`, `DELETE`

Funciones `(NOW())`

`RETURNING`

✔ Ejemplo
```bash
INSERT INTO employees(name)
VALUES ('Sarah')
RETURNING id;
```

---

## 4. MongoDB (JSON Query Format)
✔ Estructura
```json
{
  "collection": "users",
  "filter": {}
}
```
✔ Ejemplos
```json
{ "collection": "products", "filter": { "price": { "$gt": 100 } } }
```

---

## 5. Redis
✔ Estructura
```json
{ "command": "GET", "key": "name" }
```
✔ Ejemplos
```json
{ "command": "KEYS", "pattern": "*" }
```

---