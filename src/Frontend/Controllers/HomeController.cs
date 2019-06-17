using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Core;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();

        private static string API_VALUES_ROUTE = "/api/values/";
        private static string API_VALUES_RANK_ROUTE = "/api/values/rank";
        private static string TEXT_DETAILS_ROUTE ="/Home/TextDetails/";
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult TextDetails(string id)
        {
	        string rankRoute = properties["BACKEND_HOST"] + API_VALUES_ROUTE + "rank?" + id;
			string details = SendGetRequest(rankRoute).Result;
			ViewData["Message"] = details;
			return View();
		}

        [HttpPost]
        public IActionResult Upload(string message, string location)
        {
            string id = null;
            if (message == null || location == null ) {
                return Ok("Empty request.");
            }

            if (message.Contains(":"))
            {
	            return Ok("Illegal character : .");
            }

            string url = properties["BACKEND_HOST"] + API_VALUES_ROUTE;
            HttpClient client = new HttpClient();
            Console.WriteLine("User entered data: " + message + " Location: " + location);
            string str = $"{message}:{location}";
            var response = client.PostAsJsonAsync(url, str);
            id = response.Result.Content.ReadAsStringAsync().Result;
            string textDetailsRoute =
	            properties["FRONTEND_HOST"] + TEXT_DETAILS_ROUTE + "id=" + id;
            return new RedirectResult(textDetailsRoute);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> SendGetRequest(string requestUri)
		{
            HttpClient client = new HttpClient();
			var response = await client.GetAsync(requestUri);
			string value = await response.Content.ReadAsStringAsync();
			if (response.IsSuccessStatusCode && value != null)
			{
				return value;
			}
			return response.StatusCode.ToString();
		}


    }
}
