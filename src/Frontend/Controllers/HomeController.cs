using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string data)
        {
            string id = await SendRequest(data);
            Console.WriteLine("DATA: " + data);
            Console.WriteLine("ID: " + id);
            return Redirect("TextDetails/" + id);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> TextDetails(string id)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"http://127.0.01:5123/api/values/{id}");
            string letterRatio =  await response.Content.ReadAsStringAsync();
            ViewData["letterRank"] = letterRatio;

            return View();
        }

        private async Task<string> SendRequest(string data)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("http://127.0.0.1:5123/api/values", data);
            using (HttpContent responseContent = response.Content)
            {
                Task<string> res = responseContent.ReadAsStringAsync();
                string d = await res;
                return d;
            }
        }
    }
}