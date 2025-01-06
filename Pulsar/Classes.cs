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
using System.Text.Json.Serialization;

namespace Pulsar
{
    public class Meta
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Link { get; set; }
        public string ID { get; set; }
        [JsonIgnore]
        public bool IsChecked { get; set; }
        [JsonIgnore]
        public string LinkImage { get; set; }
        [JsonIgnore]
        public bool ArchiveImage { get; set; }
    }
    public class Settings
    {
        public string DeployPath { get; set; }
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
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
            }

        }
    }
}
