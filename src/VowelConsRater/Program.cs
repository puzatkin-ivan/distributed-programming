using System;
using System.Collections.Generic;
using Core;
using StackExchange.Redis;

namespace VowelConsRater
{
    class Program
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();
		private static ConnectionMultiplexer redis;

        private static string RankIdPredicate = "RANK_";
        private static string RATE_HINTS_CHANNEL = "rate_hints";
        private static string RATE_QUEUE_NAME = "rate_queue";
        
        static void Main(string[] args)
        {
            Console.WriteLine("Vowel Cons Rater is running.");
            try {
                tryToVowelConsRater();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

		private static void tryToVowelConsRater()
		{
			redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe(RATE_HINTS_CHANNEL, (RedisChannel chanel, RedisValue value) =>
            {
            	IDatabase redisDbQueue = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
                string msg = redisDbQueue.ListRightPop(RATE_QUEUE_NAME);
                while (msg != null)
                {
                	ProcessMessage(redisDbQueue, sub, msg);
                }
            });
		}

        private static void ProcessMessage(IDatabase db, ISubscriber sub, string msg)
        {
            string id = ParseData(msg, 0);

            string result = ParseData(msg, 1);
            string location = db.StringGet(id);
            Message data = new Message(result, location);

            string rankId = RankIdPredicate + id.Substring(5, id.Length - 5);
            IDatabase redisDb = redis.GetDatabase(Convert.ToInt32(data.GetDatabase()));
            redisDb.StringSet(rankId, result);
            Console.WriteLine(rankId + ": " + result + " - saved to redis. Database: " + data.GetDatabase() + " - " + location);
            msg = redisDb.ListRightPop(RATE_QUEUE_NAME);
            sub.Publish("events", $"{rankId}:{result}");
        }

        private static string ParseData(string msg, int index)
        {
            return msg.Split(':')[index];
        }
    }
}
