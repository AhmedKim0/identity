using Identity.Application.Int;

using StackExchange.Redis;

using System.Text.Json;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _connection;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connection = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _database = _connection.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue
            ? JsonSerializer.Deserialize<T>(value)
            : default;
    }

    public async Task RemoveAsync(string key)
    {
        // Safe to call even if key doesn't exist — no exception
        await _database.KeyDeleteAsync(key);
    }

    /// <summary>
    /// WATCH-like behavior: reads the current value and remembers it for transaction check.
    /// </summary>
    public async Task<bool> RunTransactionAsync<T>(string key, T newValue)
    {
        // Read current value (could be null if not exist)
        var currentValue = await _database.StringGetAsync(key);

        // Create transaction
        var tran = _database.CreateTransaction();

        // Condition: key must still have same value (null if didn't exist)
        tran.AddCondition(Condition.StringEqual(key, currentValue));

        // Operation to perform if condition passes
        var json = JsonSerializer.Serialize(newValue);
        _ = tran.StringSetAsync(key, json);

        // Execute transaction
        return await tran.ExecuteAsync();
    }
}
