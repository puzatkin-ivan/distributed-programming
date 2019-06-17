using System;
using System.Collections.Generic;
using Core;
using StackExchange.Redis;

namespace VowelConsCounter
{
    class Program
    {   
        private static Dictionary<string, string> properties = Configuration.GetParameters();
        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
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
        static void Main(string[] args)
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe("counter_hints", delegate
            {
                IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
                string msg = queueDb.ListRightPop("counter_queue");
                while (msg != null && msg != "")
                {
                    string id = msg.ToString();
                        
                    int dbNumber = Convert.ToInt32(queueDb.StringGet(id));
                    IDatabase redisDb = redis.GetDatabase(dbNumber);
                    string value = redisDb.StringGet(id);
                    
                    Dictionary<String, int> result = GetResultOfVowelsAndCons(value);
                    int vowels = result[VOWELS_STR];
                    int consonants = result[CONSONANTS_STR];
                    string resultStr = vowels + "\\" + consonants;

                    SendMessage($"{id}:{resultStr}", queueDb);
                    Console.WriteLine("Message sent => " + id + ": " + resultStr);
                    msg = queueDb.ListRightPop("counter_queue");
                }
            });
            Console.ReadLine();
        }
        private static void SendMessage(string message, IDatabase db)
        {
            db.ListLeftPush("rate_queue", message, flags: CommandFlags.FireAndForget);
            db.Multiplexer.GetSubscriber().Publish("rate_hints", "");
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
    }
}

