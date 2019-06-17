using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Threading;
using Core;
using Newtonsoft.Json;

namespace BackendApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private static string KEY_REDIS_DB = "REDIS_SERVER";
        private static string TextKeyPredicate = "Text_";
        private static Dictionary<string, string> properties = Configuration.GetParameters();
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties[KEY_REDIS_DB]);
        static readonly ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();

        [HttpGet("{rank}")]
        public IActionResult Get([FromQuery] string id)
        {
            IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
            string dbNumber = queueDb.StringGet(TextKeyPredicate + id);
            IDatabase redisDb = redis.GetDatabase(Convert.ToInt32(dbNumber));
            for (short i = 0; i < 15; ++i)
            {
                string rank = redisDb.StringGet("TextRank_" + id);
                if (rank != null)
                {
                    return Ok("Rank=" + rank + " DbNumber=" + dbNumber);
                }
                Thread.Sleep(200);
            }

            return new StatusCodeResult(402);
    }

        // POST api/values
        [HttpPost]
        public string Post([FromBody] string value)
        {
            var id = Guid.NewGuid().ToString();
            Message data = new Message(ParseData(value, 0), ParseData(value, 1));
            string textKey = TextKeyPredicate + id;
            data.SetID(textKey);
            this.SaveDataInCommonInstance(data);
            this.SaveDataInLocalInstance(data);
            this.makeEvent(data);

            return id;
        }

        private void SaveDataInLocalInstance(Message message)
        {
            var database = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
            database.StringSet(message.GetId(), message.GetDatabase());
            Console.WriteLine(message.GetId() + ": " + message.GetMessage() + " - saved to instance " + ": " + message.GetDatabase());
        }
        
        private void SaveDataInCommonInstance(Message message)
        {
            var redisConnector = ConnectionMultiplexer.Connect(properties[KEY_REDIS_DB]);
            var redisDb = redis.GetDatabase(message.GetDatabase());
            redisDb.StringSet(message.GetId(), message.GetMessage());
        }

        private void makeEvent(Message data)
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Publish("events", $"{data.GetId()}");
        }
        
        private string ParseData(string msg, int index)
        {
            return msg.Split(':')[index];
        }
    }
}