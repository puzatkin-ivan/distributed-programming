using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using StackExchange.Redis;

namespace TextRankCalc
{
    class TextRankCalcService
    {
        private static List<char> VOWELS = new List<char>(){ 'a', 'i', 'e', 'u', 'o', 'y' };
        private static List<char> CONSONANTS = new List<char>(){ 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };
        private ConnectionMultiplexer multiplexer;
        public TextRankCalcService(ConnectionMultiplexer multiplexer)
        {
            this.multiplexer = multiplexer;
        }

        public void run()
        {
            var subscriber = this.multiplexer.GetSubscriber();
            subscriber.Subscribe("events", this.onCalculateTextRank);
        }

        public void onCalculateTextRank(RedisChannel channel, RedisValue message)
        {
            var db = this.multiplexer.GetDatabase();
            string id = (string)message;
            string value = db.StringGet(id);
            
            int vowelsCount = 0;
            int consonantsCount = 0;
            bool isError = false;
            foreach (char letter in value)
            {
                if (!VOWELS.Contains(letter) && !CONSONANTS.Contains(letter))
                {
                    isError = true;
                    break;
                }
            
                vowelsCount += VOWELS.Contains(letter) ? 1 : 0;
                consonantsCount += CONSONANTS.Contains(letter) ? 1 : 0;
            }
            if (!isError)
            {
                double letterRank = (consonantsCount == 0) ? 0 : (double)vowelsCount / (double)consonantsCount;
                string idRank = "TextRank_" + id;
                db.StringSet(idRank, letterRank);
                Console.WriteLine(idRank + ": " + letterRank);
            }
        }
    }
}
