using System.Runtime.InteropServices;
using infrastructure.Services.Docker;
using Infrastructure.Services.Docker;
using infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Configure services
var services = new ServiceCollection();

// Register Configuration
services.AddSingleton<IConfiguration>(configuration);

// Configure Logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Register services
services.AddScoped<IDockerClientFactory, DockerClientFactory>();
services.AddScoped<PortFinderService>();
services.AddScoped<IDockerService, DockerService>();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Iniciando pruebas de Docker Service...");

    // Get Docker Service
    var dockerService = serviceProvider.GetRequiredService<IDockerService>();
    var portFinder = serviceProvider.GetRequiredService<PortFinderService>();

    // await TestPortFinder(portFinder, logger);
    await TestMySQL(dockerService, logger);
    //await TestSingleDatabase(dockerService, logger);
    // Descomenta la línea siguiente para probar todos los motores
    // await TestAllDatabaseEngines(dockerService, logger);

    logger.LogInformation("Todas las pruebas completadas exitosamente.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error durante las pruebas.");
}

// Keep console alive
Console.WriteLine("\nPress any key to leave.");
Console.ReadLine();

static async Task TestMySQL(IDockerService dockerService, ILogger logger)
{
    logger.LogInformation("🧪 Probando DockerService con MySQL...");
    
    try
    {
        // 1. Probar conexión con Docker
        logger.LogInformation("🔌 Probando conexión con Docker...");
        
        // 2. Crear un contenedor de MySQL
        logger.LogInformation("🐳 Creando contenedor de MySQL...");
        
        var container = await dockerService.CreateContainerAsync(
            image: "mysql:8.0",
            databaseType: "mysql", 
            credentials: "sergio:mysqlpassword123"
        );
        
        logger.LogInformation($"✅ Contenedor creado: {container.Id}");
        logger.LogInformation($"📝 Nombre: {container.Name}");
        logger.LogInformation($"🔗 Puerto: {container.HostPort}");
        logger.LogInformation($"🔄 Estado: {container.Status}");
        
        // 3. Iniciar el contenedor
        logger.LogInformation("▶️ Iniciando contenedor...");
        await dockerService.StartContainerAsync(container.Id);
        
        // MySQL necesita más tiempo para inicializarse
        logger.LogInformation("⏳ Esperando 15 segundos para inicialización de MySQL...");
        await Task.Delay(15000);
        
        // 4. Verificar estado
        var status = await dockerService.GetContainerStatusAsync(container.Id);
        logger.LogInformation($"📊 Estado actual: {status.Status}");
        
        // 5. Obtener logs
        logger.LogInformation("📋 Obteniendo logs del contenedor...");
        try 
        {
            var logs = await dockerService.GetContainerLogsAsync(container.Id);
            var logPreview = logs.Length > 500 ? logs.Substring(0, 500) + "..." : logs;
            logger.LogInformation($"📜 Logs: {logPreview}");
            
            // Verificar si MySQL está listo en los logs
            if (logs.Contains("ready for connections") || logs.Contains("MySQL init process done"))
            {
                logger.LogInformation("✅ MySQL está listo para conexiones");
            }
            else
            {
                logger.LogWarning("⚠️ MySQL podría no estar completamente inicializado");
            }
        }
        catch (Exception logEx)
        {
            logger.LogWarning($"⚠️ No se pudieron obtener logs: {logEx.Message}");
        }
        
        // 6. Probar conexión a la base de datos (opcional)
        await TestMySQLConnection(container.HostPort, logger);
        
        // 7. Detener contenedor
        logger.LogInformation("⏹️ Deteniendo contenedor...");
        await dockerService.StopContainerAsync(container.Id);
        
        // 8. Eliminar contenedor (opcional - comenta si quieres mantenerlo)
        // logger.LogInformation("🗑️ Eliminando contenedor...");
        // await dockerService.DeleteContainerAsync(container.Id);
        
        logger.LogInformation("✅ MySQL probado exitosamente!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error en DockerService con MySQL");
        
        // Mostrar ayuda adicional
        logger.LogInformation("💡 SOLUCIÓN: Ejecuta estos comandos en tu terminal:");
        logger.LogInformation("   docker pull mysql:8.0");
        logger.LogInformation("   docker images");
        logger.LogInformation("   docker ps -a");
        throw;
    }
}

static async Task TestMySQLConnection(int port, ILogger logger)
{
    logger.LogInformation($"🔗 Intentando conectar a MySQL en puerto {port}...");
    
    try
    {
        // Aquí puedes agregar código para probar la conexión real a MySQL
        // usando MySqlConnector o similar
        
        logger.LogInformation($"📡 Endpoint de MySQL: localhost:{port}");
        logger.LogInformation($"👤 Usuario: sergio");
        logger.LogInformation($"🔐 Contraseña: mysqlpassword123");
        logger.LogInformation($"📊 Base de datos: (default)");
        
        // Ejemplo de string de conexión
        var connectionString = $"Server=localhost;Port={port};User ID=sergio;Password=mysqlpassword123;";
        logger.LogInformation($"🔌 Connection string: {connectionString}");
        
        await Task.CompletedTask;
    }
    catch (Exception ex)
    {
        logger.LogWarning($"⚠️ No se pudo conectar a MySQL: {ex.Message}");
    }
}

static async Task TestPortFinder(PortFinderService portFinder, ILogger logger)
{
    logger.LogInformation("Probando PortFinderService...");

    try
    {
        var port1 = portFinder.FindAvailablePort();
        logger.LogInformation($"Puerto 1 asignado: {port1}");

        var port2 = portFinder.FindAvailablePort();
        logger.LogInformation($"Puerto 2 asignado: {port2}");

        var allocatedPorts = portFinder.GetAllocatedPorts();
        logger.LogInformation($"Puertos asignados: {allocatedPorts.Count}");

        // Release ports
        portFinder.ReleasePort(port1);
        portFinder.ReleasePort(port2);

        logger.LogInformation("PortFinderService probado exitosamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante las ports.");
        throw;
    }
}