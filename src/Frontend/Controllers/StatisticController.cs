using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Core;

namespace Frontend.Controllers
{
    
    public class StatisticsController : Controller
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();
        
        [HttpGet]
        public async Task<IActionResult> TextStatistic()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(properties["BACKEND_HOST"] + properties["BACKEND_API_GET_STATISTIC"]);
            string result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Statistics -> " + result);
            ViewData["Message"] = result;
            return View();
        }

    }
}