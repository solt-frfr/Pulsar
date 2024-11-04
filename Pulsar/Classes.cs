using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace Pulsar
{
    internal class Meta
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Link { get; set; }
        public string ID { get; set; }
    }
    public class Debug
    {
        
        List<string> kms = new List<string>();
        public void Log(string input)
        {
            kms.Add(input);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(kms, jsonoptions);
            string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\log.json";
            File.WriteAllText(filepath, jsonString);
        }
    }
}
