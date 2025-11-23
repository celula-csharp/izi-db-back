using application.DTOs;
using application.Interfaces;
using domain.Interfaces;
using System.Threading.Tasks;
using infrastructure.Factory;
using IPermissionService = domain.Interfaces.IPermissionService;

namespace application.Services
{
    public class QueryExecutor : IQueryExecutor
    {
        private readonly IDatabaseFactory _factory;

        public QueryExecutor(IDatabaseFactory factory)
        {
            _factory = factory;
        }
        
        public async Task<QueryResultDto> ExecuteQueryAsync(
            string engine,
            string query,
            string connectionString,
            string userId
        )
        {
            var result = new QueryResultDto();

            try
            {
                // 1. Crear conexión según motor
                var conn = _factory.Create(engine, connectionString);

                if (conn == null)
                {
                    result.Success = false;
                    result.Error = $"Engine '{engine}' is not supported.";
                    return result;
                }

                // 2. Abrir conexión
                await conn.Open();

                // 3. Ejecutar query
                var records = await conn.ExecuteQuery(query);

                // 4. Cerrar conexión
                await conn.Close();

                result.Success = true;
                result.Records = records ?? new List<Dictionary<string, object>>();
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }
    }
}
