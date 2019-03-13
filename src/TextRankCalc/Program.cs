using System;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");

        static void Main(string[] args)
        {
            TextRankCalcService service = new TextRankCalcService(redis);
            service.run();
            Console.ReadKey();
        }
    }
}
