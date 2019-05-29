using System;
using System.Collections.Generic;
using Core;
using StackExchange.Redis;

namespace VowelConsCounter
{
    class Program
    {   
        private static Dictionary<string, string> properties = Configuration.GetParameters();
        const string VOWELS_STR = "Vowels";
        const string CONSONANTS_STR = "Consonants";
        private static ISet<char> VOWELS = new HashSet<char>
		{
			'a', 'e', 'i', 'o', 'u', 'y'
		};

        private static ISet<char> CONSONANTS = new HashSet<char>
		{
			'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
			'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z'
		};     
        const string COUNTER_HINTS_CHANNEL = "counter_hints";
        const string COUNTER_QUEUE_NAME = "counter_queue";
        const string RATE_HINTS_CHANNEL = "rate_hints";
        const string RATE_QUEUE_NAME = "rate_queue";
        static void Main(string[] args)
        {
            Console.WriteLine("Vowel Cons Counter is running.");
            try {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
                ISubscriber sub = redis.GetSubscriber();
                sub.Subscribe(COUNTER_HINTS_CHANNEL, delegate
                {
                    IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
                    string msg = queueDb.ListRightPop(COUNTER_QUEUE_NAME);
                    while (msg != null && msg != "")
                    {
                        ProcessMessage(redis, msg);
                    }
                });
            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        private static void ProcessMessage(ConnectionMultiplexer redis, string msg)
        {
            string id = msg.ToString();
                        
            IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
            int dbNumber = Message.GetDatabaseNumber(queueDb.StringGet(id));
            IDatabase redisDb = redis.GetDatabase(dbNumber);
            string value = redisDb.StringGet(id);
                    
            Dictionary<String, int> result = GetResultOfVowelsAndCons(value);
            int vowels = result[VOWELS_STR];
            int consonants = result[CONSONANTS_STR];
            string resultStr = vowels + "\\" + consonants;

            SendMessage($"{id}:{resultStr}", queueDb);
            Console.WriteLine("Message sent: " + id + ": " + resultStr);
            msg = queueDb.ListRightPop(COUNTER_QUEUE_NAME);
        }

        private static Dictionary<String, int> GetResultOfVowelsAndCons(string text) {
            int vowels = 0;
			int consonants = 0;
            foreach (char ch in text.ToLower()) {
                if (VOWELS.Contains(ch)) {
                    ++vowels;
                } else if (CONSONANTS.Contains(ch)) {
                    ++consonants;
                } else {
                    Console.WriteLine("Unknown character: " + ch);
                }
            }
            Dictionary<String, int> result = new Dictionary<string, int>();
            result.Add(VOWELS_STR, vowels);
            result.Add(CONSONANTS_STR, consonants);
            return result;
        }

        private static void SendMessage(string message, IDatabase db)
        {
            // put message to queue
            db.ListLeftPush(RATE_QUEUE_NAME, message, flags: CommandFlags.FireAndForget);
            // and notify consumers
            db.Multiplexer.GetSubscriber().Publish(RATE_HINTS_CHANNEL, "");
        }
    }
}

