using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pulsar.Utitlities
{
    public static class PathManagement
    {
        public static void GatherFiles(string sender)
        {
            List<string> Paths = new List<string>();
            try
            {
                // Get all files in the folder
                string[] files = Directory.GetFiles(sender, "*.*", SearchOption.AllDirectories);

                // Iterate and print each file path
                foreach (string file in files)
                {
                    string filetrim = file.Replace(sender, "");
                    if (!filetrim.Contains("meta.json") && !filetrim.Contains("files.json") && !filetrim.Contains("deploy.json"))
                        Paths.Add(filetrim);
                }
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(Paths, jsonoptions);
                string filepath = sender + $@"\files.json";
                File.WriteAllText(filepath, jsonString);
                AemulusModManager.Utilities.ParallelLogger.Log($"[INFO] files.json written to {filepath}.");
            }
            catch (Exception ex)
            {
                AemulusModManager.Utilities.ParallelLogger.Log($"[ERROR] {ex.Message}");
            }
        }
    }
}
