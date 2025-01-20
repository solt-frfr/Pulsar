using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Quasar_Rewrite.Properties;

namespace Pulsar.Utitlities
{
    public static class Conflict
    {
        public static bool Detect(string sender, string ID)
        {
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            string jsonString = System.IO.File.ReadAllText(settingspath);
            bool accept = false;
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            foreach (string checkID in Directory.GetDirectories(settings.DeployPath))
            {
                string purecheckID = Path.GetFileName(checkID);
                if (File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ID}\{sender}") 
                    && File.Exists($@"{checkID}\{sender}")
                    && purecheckID != ID
                    && Path.GetExtension(sender) != ".toml" 
                    && Path.GetExtension(sender) != ".json"
                    && Path.GetExtension(sender) != ".webp")
                {
                    Alert aw = new Alert($"There's a conflict between {purecheckID} and {ID}.\nCancel will keep {purecheckID}'s file.\nConfirm will use {ID}'s file.", false);
                    accept = true;
                    aw.OnAlertHandled = () =>
                    {
                        accept = false;
                        File.Delete($@"{checkID}\{sender}");
                    };
                    string temp = Path.GetExtension(sender);
                    aw.ShowDialog();
                }
            }
            if (accept)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
