using StackExchange.Redis;

namespace Identity.Infrastructure.Redis
{


    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService(IConfiguration configuration)
        {
            var redisConfig = configuration.GetSection("Redis");

            var options = new ConfigurationOptions
            {
                EndPoints = { $"{redisConfig["Hosts:0:Host"]}:{redisConfig["Hosts:0:Port"]}" },
                Password = redisConfig["password"],
                ConnectTimeout = int.Parse(redisConfig["ConnectTimeout"]),
                ConnectRetry = int.Parse(redisConfig["ConnectRetry"]),
                Ssl = bool.Parse(redisConfig["Ssl"]),
                AllowAdmin = bool.Parse(redisConfig["AllowAdmin"]),
                ResolveDns = bool.Parse(redisConfig["resolveDns"])
            };

            _redis = ConnectionMultiplexer.Connect(options);
            _db = _redis.GetDatabase(int.Parse(redisConfig["Database"]));
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            await _db.StringSetAsync(key, value, expiry);
        }

        public async Task<string?> GetAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }
    }

}
