using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheSharp.Caching
{
    public sealed class RedisAsyncCache : AsyncCache<string>
    {
        private IDatabase db;
       

        public override async Task Initialize(Dictionary<string, string> parameters)
        {
            var endpoint = parameters["Endpoint"];
            var redis = await ConnectionMultiplexer.ConnectAsync(endpoint);
            db = redis.GetDatabase();
        }

        public override List<string> InitializationProperties
        {
            get { return new List<string> {"Endpoint"}; }
        }

        protected override async Task Put(string key, string value, TimeSpan lifeSpan)
        {
            await db.StringSetAsync(key, value, lifeSpan);
        }

        protected override async Task<string> Get(string key)
        {
            return await db.StringGetAsync(key);
        }

        protected override async Task Remove(string key)
        {
            await db.KeyDeleteAsync(key);
        }
    }
}