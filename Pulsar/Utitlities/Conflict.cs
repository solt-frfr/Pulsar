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
        public static bool Detect(string sender, string ID, string source)
        {
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            string conflictpath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\conflictlog.json";
            string jsonString = System.IO.File.ReadAllText(settingspath);
            List<List<string>> log = new List<List<string>>();
            bool accept = false;
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            if (File.Exists(conflictpath))
            {
                jsonString = System.IO.File.ReadAllText(conflictpath);
                log = JsonSerializer.Deserialize<List<List<string>>>(jsonString, jsonoptions);
            }
            foreach (string checkID in Directory.GetDirectories(settings.DeployPath))
            {
                string purecheckID = Path.GetFileName(checkID);
                if (File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ID}\{source}") 
                    && File.Exists($@"{checkID}\{sender}")
                    && purecheckID != ID
                    && Path.GetExtension(sender).ToLower() != ".toml" 
                    && Path.GetFileName(sender).ToLower() != "config.json"
                    && Path.GetFileName(sender).ToLower() != "userconfig.json"
                    && Path.GetFileName(sender).ToLower() != "files.json"
                    && Path.GetFileName(sender).ToLower() != "deploy.json"
                    && Path.GetFileName(sender).ToLower() != "meta.json"
                    && Path.GetFileName(sender).ToLower() != "preview.webp"
                    && Path.GetFileName(sender).ToLower() != "info.ini"
                    && Path.GetFileName(sender).ToLower() != "readme.txt")
                {
                    ConflictWindow cw = new ConflictWindow(sender, ID, source, checkID);
                    accept = true;
                    cw.OnConflictHandled = () =>
                    { 
                        accept = false;
                        File.Delete($@"{checkID}\{sender}");
                    };
                    string temp = Path.GetExtension(sender);
                    List<string> conflict = new List<string>();
                    conflict.Add(System.IO.Path.GetFileName(checkID) + sender);
                    conflict.Add(ID + source);
                    log.Add(conflict);
                    cw.ShowDialog();
                }
            }
            jsonString = JsonSerializer.Serialize(log, jsonoptions);
            System.IO.File.WriteAllText(conflictpath, jsonString);
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
