using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Core
{
    public class Configuration
    {
        public static Dictionary<string, string> GetParameters()
        {
            string filePath = Directory.GetCurrentDirectory() + "/properties.json";
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));

            return json;
        }

    }
}