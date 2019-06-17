using System;
using System.Collections.Generic;
using Core;
using StackExchange.Redis;

namespace VowelConsRaterc
{
    class Program
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();  
        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
        public static IDatabase database = redis.GetDatabase(Convert.ToInt32(4));       
        static void Main(string[] args)
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe("rate_hints", delegate
            {
                string msg = database.ListRightPop("rate_queue");
                while (msg != null)
                {
                    string id = msg.Split(':')[0];
                    string result = msg.Split(':')[1];
                    string location = database.StringGet(id);
                    Message data = new Message(result, location);
                    string rankId = "TextRank_" + id.Substring(5, id.Length - 5);
                    IDatabase redisDb = redis.GetDatabase(Convert.ToInt32(data.GetDatabase()));
                    redisDb.StringSet(rankId, result);
                    Console.WriteLine(rankId + ": " + result + " - saved. Database: " + data.GetDatabase() + " - " + location);
                    msg = redisDb.ListRightPop("rate_queue");
                    sub.Publish("events", $"{rankId}:{result}");
                }
            });
            
            Console.ReadLine();
        }
    }
}
