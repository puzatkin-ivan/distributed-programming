using System;
using StackExchange.Redis;
using System.Configuration;

namespace TextListener
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Console Text Listener is running.");
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
                IDatabase redisDb = redis.GetDatabase();
                ISubscriber sub = redis.GetSubscriber();
                sub.Subscribe("textCreated", (channel, message) => 
                {
                    Console.WriteLine("TextCreated: " +  message);
                });
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
