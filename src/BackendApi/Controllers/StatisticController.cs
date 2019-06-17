using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Core;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace BackendApi.Controllers
{
    [Route("api/[controller]")]
    public class StatisticController : Controller
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();

        [HttpGet("{text_statistic}")]
        public IActionResult Get()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(properties["REDIS_SERVER"]);
            IDatabase queueDb = redis.GetDatabase(Convert.ToInt32(properties["COMMON_DB"]));
            for (short i = 0; i < 10; ++i)
            {
                string statistic = queueDb.StringGet("text_statistic");
                if (!String.IsNullOrEmpty(statistic))
                {
					return Ok(statistic);
                }
				Thread.Sleep(200);
            }

            return new NotFoundResult();
        }

        // POST api/statistic
        [HttpPost]
        public string Post([FromBody] string value)
        {
            return null;
        }

    }
}