using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Threading;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private static readonly ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();
        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get([FromRoute]string id)
        {
            string value = null;
            IDatabase db = redis.GetDatabase();
            
            for (int i = 0; i < 10; i++)
            {
                value = db.StringGet("TextRank_" + id);

                if (value == null)
                {
                    value = "Not Found value by id";
                    Thread.Sleep(300);
                    continue;
                }
                break;
            }
            return value;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]string value)
        {
            string id = Guid.NewGuid().ToString();
            _data[id] = value;
            IDatabase db = redis.GetDatabase();
            db.StringSet(id, value);
            var subscriber = redis.GetSubscriber();
            subscriber.Publish("events", id);
            return id;
        }
    }
}
