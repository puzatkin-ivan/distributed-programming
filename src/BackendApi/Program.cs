using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Core;
using Microsoft.Extensions.Logging;

namespace BackendApi
{
    public class Program
    {
        private static Dictionary<string, string> properties = Configuration.GetParameters();

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(properties["BACKEND_HOST"])
                .Build();
    }
}