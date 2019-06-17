using System;
using System.Collections.Generic;
using Core;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();
        
        const string COUNTER_HINTS_CHANNEL = "counter_hints";
        const string COUNTER_QUEUE_NAME = "counter_queue";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Text rank calculator is running.");
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
                ISubscriber sub = redis.GetSubscriber();
                sub.Subscribe("events", (channel, message) =>
                {
                    string[] elements = message.ToString().Split(":");
                    if (elements.Length != 2)
                    {
                        return;
                    }
                    string id = elements[0];
                    bool isAccepted = Convert.ToBoolean(elements[1]);
                    if (!isAccepted)
                    {
                        Console.WriteLine("No access.");
                        return;
                    }
                    if (id.Contains("Text_"))
                    {
                        IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
                        int dbNumber = Convert.ToInt32(queueDb.StringGet(id));
                        IDatabase redisDb = redis.GetDatabase(dbNumber);
                        string value = redisDb.StringGet(id);
                        SendMessage($"{id}", queueDb);
                        Console.WriteLine("Message sent => " + id + ": " + value);
                    }
                });
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void SendMessage(string message, IDatabase db)
        {
            db.ListLeftPush("counter_queue", message, flags: CommandFlags.FireAndForget);
            db.Multiplexer.GetSubscriber().Publish("counter_hints", "");
        }
    }
}