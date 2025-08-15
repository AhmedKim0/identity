using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Redis
{
    public class RedisSettings
    {
        public bool Enabled { get; set; }
        public bool AllowAdmin { get; set; }
        public bool Ssl { get; set; }
        public int ConnectTimeout { get; set; }
        public int ConnectRetry { get; set; }
        public int Database { get; set; }
        public List<RedisHost> Hosts { get; set; }
        public string Password { get; set; }
        public bool ResolveDns { get; set; }
    }

    public class RedisHost
    {
        public string Host { get; set; }
        public string Port { get; set; }
    }

}
