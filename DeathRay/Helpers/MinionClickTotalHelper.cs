using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using DeathRay.Models;

namespace DeathRay.Helpers
{
    public static class MinionClickTotalHelper
    {
        private static ConnectionMultiplexer redisConnection;
        private static ConnectionMultiplexer RedisConnection
        {
            get
            {
                if (redisConnection == null || !redisConnection.IsConnected)
                {
                    redisConnection = ConnectionMultiplexer.Connect("deathray.redis.cache.windows.net,password=u5DcJnI3cD4L39Ol1mnsqDQPPcteEzqbATxIouk92qk=");
                }
                return redisConnection;
            }
        }

        public static MinionClickTotal GetMinionClickTotal(string username)
        {
            IDatabase cache = RedisConnection.GetDatabase();
            long total = (long)cache.StringGet("MinionTotal-" + username);
            return new MinionClickTotal() { Minion = username, ClickTotal = total };
        }
    }
}