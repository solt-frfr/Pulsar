using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AemulusModManager.Utilities;
using static System.Net.Mime.MediaTypeNames;

namespace Pulsar.Utilities
{
    public static class Parser
    {
        public static Meta InfoTOML(string filepath)
        {
            Meta regenerated = new Meta();
            if (Path.GetExtension(filepath) == ".toml")
            {
                string[] fileContent = File.ReadAllLines(filepath);
                int lines = fileContent.Length;
                int startIndex = 0;
                int endIndex = 0;
                for (int i = 0; i < lines; i++)
                {
                    if (fileContent[i].StartsWith("display_name"))
                    {
                        startIndex = fileContent[i].IndexOf("\"") + 1;
                        endIndex = fileContent[i].IndexOf("\"", startIndex);
                        regenerated.Name = fileContent[i].Substring(startIndex, endIndex - startIndex);
                    }
                    if (fileContent[i].StartsWith("authors"))
                    {
                        int authorcount = fileContent[i].Split(',').Length;
                        regenerated.Authors = new List<string>();
                        for (int ii = 1; ii <= authorcount; ii++)
                        {
                            if (ii == 1 && ii == authorcount)
                            {
                                startIndex = fileContent[i].IndexOf("\"") + 1;
                                endIndex = fileContent[i].IndexOf("\"", startIndex);
                                regenerated.Authors.Add(fileContent[i].Substring(startIndex, endIndex - startIndex));
                            }
                            else if (ii == 1)
                            {
                                startIndex = fileContent[i].IndexOf("\"") + 1;
                                endIndex = fileContent[i].IndexOf(",", startIndex);
                                regenerated.Authors.Add(fileContent[i].Substring(startIndex, endIndex - startIndex));
                            }
                            else if (ii == authorcount)
                            {
                                startIndex = endIndex + 2;
                                endIndex = fileContent[i].IndexOf("\"", startIndex);
                                regenerated.Authors.Add(fileContent[i].Substring(startIndex, endIndex - startIndex));
                            }
                            else
                            {
                                startIndex = endIndex + 2;
                                endIndex = fileContent[i].IndexOf(",", startIndex);
                                regenerated.Authors.Add(fileContent[i].Substring(startIndex, endIndex - startIndex));
                            }
                        }
                    }
                    if (fileContent[i].StartsWith("version"))
                    {
                        startIndex = fileContent[i].IndexOf("\"") + 1;
                        endIndex = fileContent[i].IndexOf("\"", startIndex);
                        regenerated.Version = fileContent[i].Substring(startIndex, endIndex - startIndex);
                    }
                    if (fileContent[i].StartsWith("description"))
                    {
                        regenerated.Description = "";
                        for (int ii = i + 1; fileContent[ii] != "\"\"\""; ii++)
                        {
                            regenerated.Description += fileContent[ii];
                            if (fileContent[ii + 1] != "\"\"\"")
                            {
                                regenerated.Description += "\n";
                            }
                        }
                    }
                    if (fileContent[i].StartsWith("category"))
                    {
                        regenerated.InfoCat = MakePack.InfoCatEnum(fileContent[i]);
                    }
                }
                ParallelLogger.Log($"[INFO] Metadata generated from info.toml, please fill the rest out.");
            }
            else
            {
                ParallelLogger.Log("[ERROR] Please provide a valid toml file.");
            }
            return regenerated;
        }
    }
}