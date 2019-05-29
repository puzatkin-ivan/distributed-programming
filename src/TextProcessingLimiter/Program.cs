using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using StackExchange.Redis;

namespace TextProcessingLimiter
{
    class Program
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Text processing limiter is running");
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
                ISubscriber sub = redis.GetSubscriber();
                int countWords = 0;
                sub.Subscribe("events", (channel, message) =>
                {
                    string id = message.ToString();
                    if (id.Contains("TEXT_"))
                    {
                        if (message.ToString().Split(":").Length > 1)
                        {
                            return;
                        }
                        Console.WriteLine(message);
                        IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
                        countWords++;
                        bool result = countWords <= Convert.ToInt32(properties["TEXT_PROCESSING_LIMIT"]);
                        sub.Publish("events", (id + ":" + result.ToString()));
                        if (!result)
                        {
                            Task.Run(async () =>
                            {
                                await Task.Delay(Convert.ToInt32(properties["ACCESS_UPDATE"]) * 1000);
                                countWords = 0;
                            });
                        }
                    }

                    if (id.Contains("RANK_"))
                    {
                        Console.WriteLine(message);
                        string value = ParseData(message, 1);
                        string[] values = value.Split('\\');
                        double convertedResult = values[1] == "0"
                            ? Convert.ToDouble(values[0])
                            : Convert.ToDouble(values[0]) / Convert.ToDouble(values[1]);
                        if (convertedResult <= 0.5)
                        {
                            countWords--;
                        }
                    }
                });
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static string ParseData(string msg, int index)
        {
            return msg.Split(':')[index];
        }
    }
}