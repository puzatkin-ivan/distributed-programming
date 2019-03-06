using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;

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
        public string Get(string id)
        {
            string value = null;
            _data.TryGetValue(id, out value);
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
