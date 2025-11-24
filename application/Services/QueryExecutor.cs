using application.DTOs;
using application.Interfaces;
using domain.Interfaces;
using System.Threading.Tasks;
using domain.Enums;
using infrastructure.Factory;

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

        public async Task<object> GetSchemaAsync(DatabaseType engine, string connectionString, string userId)
        {
            try
            {
                // Convertir DatabaseType a string para el factory
                string engineString = engine.ToString().ToLower();
                
                // 1. Crear conexión según motor
                var conn = _factory.Create(engineString, connectionString);

                if (conn == null)
                {
                    return new { Error = $"Engine '{engine}' is not supported." };
                }

                // 2. Abrir conexión
                await conn.Open();

                // 3. Obtener el schema
                var schema = await conn.GetSchemaAsync();

                // 4. Cerrar conexión
                await conn.Close();

                return schema;
            }
            catch (Exception ex)
            {
                return new { Error = $"Error getting schema: {ex.Message}" };
            }
        }
    }
}