using domain.Interfaces;
using infrastructure.Factory;

namespace application.Services
{
    public class SchemaService : ISchemaService
    {
        private readonly IDatabaseFactory _factory;

        public SchemaService(IDatabaseFactory factory)
        {
            _factory = factory;
        }

        public async Task<Dictionary<string, List<Dictionary<string, object>>>?> GetSchemaAsync(
            string engine,
            string connectionString)
        {
            var conn = _factory.Create(engine, connectionString);

            if (conn == null)
                return null;

            await conn.Open();

            // Consulta de metadata por motor
            string query = engine switch
            {
                "sqlserver" => @"
                    SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
                    FROM INFORMATION_SCHEMA.COLUMNS
                ",
                "mysql" => @"
                    SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE 
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                ",
                "postgresql" => @"
                    SELECT table_name AS TABLE_NAME,
                           column_name AS COLUMN_NAME,
                           data_type AS DATA_TYPE
                    FROM information_schema.columns
                ",
                _ => null!
            };

            if (query == null)
                return null;

            var rows = await conn.ExecuteQuery(query);
            await conn.Close();

            // Agrupar tabla → columnas
            var result = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var row in rows)
            {
                var table = row["TABLE_NAME"].ToString()!;

                if (!result.ContainsKey(table))
                    result[table] = new List<Dictionary<string, object>>();

                result[table].Add(row);
            }

            return result;
        }
    }
}
