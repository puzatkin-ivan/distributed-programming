using System;
using System.Collections.Generic;
using Core;
using StackExchange.Redis;

namespace TextStatistics
{
    class Program
    {
        
        private static Dictionary<string, string> properties = Configuration.GetParameters();

        private static int textNum = 0;

        private static int highRankPart = 0;

        private static double avgRank = 0;

        private static double result = 0;

        private static int numberRejectedEvents = 0;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Text statistic is running.");
            try {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
                ISubscriber sub = redis.GetSubscriber();
                sub.Subscribe("events", (channel, message) =>
                {
                    string[] elements = message.ToString().Split(":");
                    string id = elements[0];
                    if (id.Contains("Text_"))
                    {
                        if (elements.Length != 2)
                        {
                            return;
                        } 
                        bool isAccepted = Convert.ToBoolean(elements[1]);
                        if (!isAccepted)
                        {
                            numberRejectedEvents++;
                            SaveDataInCommonInstance(redis, GetResult());
                        }
                    }

                    if (id.Contains("TextRank_"))
                    {
                        string value = ParseData(message, 1);
                        string[] values = value.Split('\\');
                        double convertedResult = values[1] == "0"
                            ? Convert.ToDouble(values[0])
                            : Convert.ToDouble(values[0]) / Convert.ToDouble(values[1]);
                        result += convertedResult;

                        ++textNum;
                        if (convertedResult > 0.5)
                        {
                            ++highRankPart;
                        }

                        avgRank = result / textNum;

                        SaveDataInCommonInstance(redis, GetResult());

                        Console.WriteLine(GetResult());
                    }
                });
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        private static void SaveDataInCommonInstance(ConnectionMultiplexer redis, string data)
        {
            var redisDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
            redisDb.StringSet("text_statistic", data);
        }

        private static string[] ParseList(string msg)
        {
            return msg.Split(":");
        }

        private static string ParseData(string msg, int index)
        {
            return msg.Split(':')[index];
        }

        private static string GetResult()
        {
            return "TextNum: " + textNum + ", AvgRank: " + avgRank + ", NumberRejectedEvents: " + numberRejectedEvents + ", HighRankPart: " + highRankPart;
        }
    }
}