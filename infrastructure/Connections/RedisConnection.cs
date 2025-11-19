using domain.Interfaces;

namespace infrastructure.Connections;

public class RedisConnection : IDatabaseConnection
{
    public Task Open()
    {
        throw new NotImplementedException();
    }

    public Task Close()
    {
        throw new NotImplementedException();
    }

    public Task<string> ExecuteQuery(string query)
    {
        throw new NotImplementedException();
    }
}