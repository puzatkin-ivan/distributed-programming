﻿using System;
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
            string id = await GetData(data);
            Console.WriteLine("DATA: " + data);
            Console.WriteLine("ID: " + id);
            return Ok(id);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> GetData(string data)
        {
            HttpContent content = new StringContent(data);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5123/api/values", content);
            using (HttpContent responseContent = response.Content)
            {
                Task<string> res = responseContent.ReadAsStringAsync();
                string d = await res;
                return d;
            }
        }
    }
}