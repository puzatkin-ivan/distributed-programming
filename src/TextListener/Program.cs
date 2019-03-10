using System;
using StackExchange.Redis;

namespace TextListener
{
    class Program
    {
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        static void Main(string[] args)
        {
            var sub = redis.GetSubscriber();
            sub.Subscribe("events", (channel, id) => {
                var db = redis.GetDatabase();
                var message = db.StringGet((string)id);
                Console.WriteLine("id: " + (string)id);
                Console.WriteLine("message: " + (string)message);
            });
            Console.ReadLine();
        }
    }
}