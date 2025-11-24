# Configuración del Entorno de Desarrollo con Docker

Esta guía describe cómo configurar las dependencias de bases de datos para el proyecto IZI-DB utilizando Docker.

---

## 1. Requisitos Previos

Asegúrate de tener instalado y en ejecución:

*   **Docker Desktop**: [Descargar desde el sitio oficial de Docker](https://www.docker.com/products/docker-desktop/)

Puedes verificar que Docker está corriendo con el comando:
```bash
docker --version
```

---

## 2. Levantar Contenedores de Bases de Datos

Ejecuta los siguientes comandos en tu terminal para descargar y ejecutar las imágenes de las bases de datos necesarias. Se han añadido sufijos `-dev` a los nombres para evitar conflictos.

### SQL Server
```bash
docker run --name mssql-dev -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Admin123*" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### MySQL
```bash
docker run --name mysql-dev -e MYSQL_ROOT_PASSWORD=admin -p 3306:3306 -d mysql:8
```

### PostgreSQL
```bash
docker run --name postgres-dev -e POSTGRES_PASSWORD=admin -p 5432:5432 -d postgres
```

### MongoDB
```bash
docker run --name mongo-dev -p 27017:27017 -d mongo
```

### Redis
```bash
docker run --name redis-dev -p 6379:6379 -d redis
```

---

## 3. Verificar Contenedores

Para asegurarte de que todos los contenedores se están ejecutando correctamente, puedes usar el siguiente comando:

```bash
docker ps
```

Deberías ver en la lista los contenedores `mssql-dev`, `mysql-dev`, `postgres-dev`, `mongo-dev`, y `redis-dev`.

---

## 4. Configuración de Conexiones

El archivo `api/appsettings.json` debe estar configurado para apuntar a estas instancias de Docker. Las cadenas de conexión correspondientes son:

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

Con estos pasos, el entorno de bases de datos estará listo para que la aplicación se conecte y funcione correctamente.
