using Docker.DotNet;
using Docker.DotNet.Models;
using domain.Models.Docker;
using infrastructure.Services.Interfaces;
using infrastructure.Services.Docker;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Docker
{
    public class DockerService : IDockerService
    {
        private readonly IDockerClientFactory _clientFactory;
        private readonly PortFinderService _portFinder;
        private readonly ILogger<DockerService> _logger;

        public DockerService(
            IDockerClientFactory clientFactory,
            PortFinderService portFinder,
            ILogger<DockerService> logger)
        {
            _clientFactory = clientFactory;
            _portFinder = portFinder;
            _logger = logger;
        }

        public async Task<DockerContainer> CreateContainerAsync(string image, string databaseType, string credentials)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var hostPort = _portFinder.FindAvailablePort();
                var containerPort = GetContainerPort(databaseType);

                _logger.LogInformation("Creando contenedor con imagen: {Image}, puerto: {Port}", image, hostPort);

                // Configurar parámetros del contenedor
                var containerParameters = new CreateContainerParameters
                {
                    Image = image,
                    Name = $"db-{databaseType.ToLower()}-{Guid.NewGuid().ToString()[..8]}",
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                $"{containerPort}/tcp",
                                new List<PortBinding> { new() { HostPort = hostPort.ToString() } }
                            }
                        }
                    },
                    Env = GetEnvironmentVariables(databaseType, credentials)
                };

                // Crear el contenedor
                var response = await client.Containers.CreateContainerAsync(containerParameters);
                
                _logger.LogInformation("Contenedor creado con ID: {ContainerId}", response.ID);

                return new DockerContainer
                {
                    Id = response.ID,
                    Name = containerParameters.Name,
                    Image = image,
                    HostPort = hostPort,
                    ContainerPort = containerPort,
                    DatabaseType = databaseType,
                    Status = "Created",
                    CreatedAt = DateTime.UtcNow,
                    ConnectionString = GenerateConnectionString(databaseType, hostPort, credentials)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear contenedor con imagen: {Image}", image);
                throw;
            }
        }

        public async Task StartContainerAsync(string containerId)
        {
            using var client = _clientFactory.CreateClient();
            await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
            _logger.LogInformation("Contenedor {ContainerId} iniciado", containerId);
        }

        public async Task StopContainerAsync(string containerId)
        {
            using var client = _clientFactory.CreateClient();
            await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
            _logger.LogInformation("Contenedor {ContainerId} detenido", containerId);
        }

        public async Task DeleteContainerAsync(string containerId)
        {
            using var client = _clientFactory.CreateClient();
            await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { Force = true });
            
            // Liberar el puerto (necesitarías mapear containerId -> port)
            _logger.LogInformation("Contenedor {ContainerId} eliminado", containerId);
        }

        public async Task<string> GetContainerLogsAsync(string containerId)
        {
            using var client = _clientFactory.CreateClient();
            var logs = await client.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Timestamps = true
            });

            using var reader = new StreamReader(logs);
            return await reader.ReadToEndAsync();
        }

        public async Task<DockerContainer> GetContainerStatusAsync(string containerId)
        {
            using var client = _clientFactory.CreateClient();
            var container = await client.Containers.InspectContainerAsync(containerId);

            return new DockerContainer
            {
                Id = container.ID,
                Name = container.Name,
                Image = container.Config.Image,
                Status = container.State.Status,
                State = container.State.Status,
                CreatedAt = container.Created
            };
        }

        #region Private Methods
        private int GetContainerPort(string databaseType)
        {
            return databaseType.ToLower() switch
            {
                "postgresql" => 5432,
                "mysql" => 3306,
                "sqlserver" => 1433,
                "mongodb" => 27017,
                "redis" => 6379,
                _ => 5432
            };
        }

        private List<string> GetEnvironmentVariables(string databaseType, string credentials)
        {
            var envVars = new List<string>();
            
            switch (databaseType.ToLower())
            {
                case "postgresql":
                    envVars.AddRange(new[]
                    {
                        "POSTGRES_DB=studentdb",
                        $"POSTGRES_USER={credentials.Split(':')[0]}",
                        $"POSTGRES_PASSWORD={credentials.Split(':')[1]}"
                    });
                    break;
                case "mysql":
                    envVars.AddRange(new[]
                    {
                        "MYSQL_DATABASE=studentdb",
                        $"MYSQL_USER={credentials.Split(':')[0]}",
                        $"MYSQL_PASSWORD={credentials.Split(':')[1]}",
                        "MYSQL_RANDOM_ROOT_PASSWORD=yes"
                    });
                    break;
            }
            
            return envVars;
        }

        private string GenerateConnectionString(string databaseType, int hostPort, string credentials)
        {
            var user = credentials.Split(':')[0];
            var password = credentials.Split(':')[1];
            
            return databaseType.ToLower() switch
            {
                "postgresql" => $"Host=localhost;Port={hostPort};Database=studentdb;Username={user};Password={password}",
                "mysql" => $"Server=localhost;Port={hostPort};Database=studentdb;Uid={user};Pwd={password}",
                "sqlserver" => $"Server=localhost,{hostPort};Database=studentdb;User Id={user};Password={password}",
                "mongodb" => $"mongodb://{user}:{password}@localhost:{hostPort}/studentdb",
                "redis" => $"localhost:{hostPort},password={password}",
                _ => string.Empty
            };
        }
        #endregion
    }
}