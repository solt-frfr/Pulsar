using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AemulusModManager;
using AemulusModManager.Utilities;
using Pulsar.Utilities;
using SevenZipExtractor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string fileext;
        private string pagelink;
        private string filelink;
        private List<string> fromhtml = new List<string>();
        private List<string> authors = new List<string>();
        private bool cancel = false;
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] runningProcesses = Process.GetProcessesByName(processName);
            foreach (var process in runningProcesses)
            {
                if (process.Id != Process.GetCurrentProcess().Id)
                {
                    Console.WriteLine($"Closing process: {process.Id}");
                    process.Kill();
                    process.WaitForExit();
                }
            }
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2");
            Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2", true);
            bool isNewInstance;
            bool oneClick = e.Args.Length > 0;
            MainWindow mw = new MainWindow(false, oneClick);
            mw.Show();
            if (!oneClick)
            {
                ParallelLogger.Log($@"[DEBUG] Pulsar opened normally.");
            }
            if (oneClick)
            {
                await HandleCustomProtocol(e.Args[0]);
            }
            mw.Refresh();
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        private async Task Get(string page)
        {
            string url = page;
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2");
            string filePath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2\";
            try
            {
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(url, filePath + "mod.html");

                    ParallelLogger.Log("[INFO] Download complete!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.Load(filePath + "mod.html");
                var anchorTags = htmlDoc.DocumentNode.SelectNodes("//meta[@property='og:image']");
                if (anchorTags != null)
                {
                    foreach (var tag in anchorTags)
                    {
                        WebClient webClient = new WebClient();
                        string hrefValue = tag.GetAttributeValue("content", "No content found");
                        webClient.DownloadFile(hrefValue, filePath + "preview");
                        ParallelLogger.Log($"[INFO] Preview image downloaded and saved to {filePath + "preview"}");
                    }
                }
                anchorTags = htmlDoc.DocumentNode.SelectNodes($"//a[@href='{url}']");
                if (anchorTags != null)
                {
                    foreach (var tag in anchorTags)
                    {
                        fromhtml.Add(tag.InnerText);
                        string idtext = tag.InnerText.Trim();
                        idtext = idtext.ToLower();
                        idtext = idtext.Replace(" ", string.Empty);
                        idtext = FixInvalid(idtext);
                        fromhtml.Add(idtext);
                        break;
                    }
                }
                anchorTags = htmlDoc.DocumentNode.SelectNodes($"//a");
                if (anchorTags != null)
                {
                    foreach (var tag in anchorTags)
                    {
                        try
                        {
                            string hrefValue = tag.GetAttributeValue("href", string.Empty);
                            if (hrefValue.Contains("https://gamebanana.com/mods/cats/") && hrefValue != "")
                            {
                                fromhtml.Add(tag.InnerText);
                            }
                        }
                        catch
                        {
                            fromhtml.Add("");
                        }
                    }
                }
                if (fromhtml.Count == 2)
                {
                    fromhtml.Add("");
                }
                if (fromhtml.Count == 3)
                {
                    fromhtml.Add("");
                }
                anchorTags = htmlDoc.DocumentNode.SelectNodes("//meta[@property='og:description']");
                if (anchorTags != null)
                {
                    foreach (var tag in anchorTags)
                    {
                        WebClient webClient = new WebClient();
                        string hrefValue = tag.GetAttributeValue("content", "No content found");
                        fromhtml.Add(hrefValue);
                        if (hrefValue.Contains("submitted by "))
                        {
                            int startIndex = hrefValue.IndexOf("submitted by ") + 13;
                            int endIndex = hrefValue.Length;
                            string auth = hrefValue.Substring(startIndex, endIndex - startIndex);
                            if (auth.Contains(" and ")) 
                            {
                                authors.Add(auth.Substring(0, auth.IndexOf(" and ")));
                                startIndex = auth.IndexOf(" and ") + 5;
                                endIndex = auth.Length;
                                authors.Add(auth.Substring(startIndex, endIndex - startIndex));
                            }
                            else
                                authors.Add(auth);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ParallelLogger.Log($@"[ERROR] An error occurred: {ex.Message}");
            }
        }
        private async Task HandleCustomProtocol(string url)
        {
            if (url.StartsWith("quasar:") || url.StartsWith("pulsar:"))
            {
                ParallelLogger.Log($@"[INFO] Handling custom protocol: {url}");
                FindLinks(url);
                await Get(pagelink);
                Alert aw = new Alert(url, true);
                aw.OnAlertHandled = async () =>
                {
                    await Download(filelink);
                    if (!cancel)
                        await Install1C();
                    else
                        return;
                };
                aw.Update(fromhtml[0], $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2\preview");
                aw.ShowDialog();
            }
            else
            {
                ParallelLogger.Log("[ERROR] Unexpected URL format.");
            }
        }

        public static string FixInvalid(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars()
                                     .Concat(System.IO.Path.GetInvalidPathChars())
                                     .Distinct()
                                     .ToArray();
            return new string(input.Where(c => !invalidChars.Contains(c)).ToArray());
        }
        private void FindLinks(string sender)
        {
            pagelink = $"https://gamebanana.com/mods/{sender.Split(',')[2]}";
            filelink = sender.Replace("quasar:", "");
            filelink = filelink.Replace("pulsar:", "");
            filelink = filelink.Split(',')[0];
            fileext = sender.Split(',')[3];
        }
        private async Task Download(string sender)
        {
            string temppath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2";
            Directory.CreateDirectory(temppath);
            try
            {
                WebClient webClient = new WebClient();
                string destinationPath = temppath + $@"\{fromhtml[1]}.{fileext}";
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(destinationPath)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destinationPath));
                }
                webClient.DownloadFile(filelink, destinationPath);

                ParallelLogger.Log("[INFO] Download complete!");
            }
            catch (Exception ex)
            {
                ParallelLogger.Log($"[ERROR] An error occurred: {ex.Message}");
            }
        }

        private async Task Install1C()
        {
            string mod = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2\{fromhtml[1]}.{fileext}";
            try
            {
                SevenZip.SevenZipExtractor.SetLibraryPath($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\x86\7z.dll");
                string archivePath = null;
                string filename = null;
                string filetype = null;
                string firstDirectoryPath = null;
                string extpath = null;
                string newpath = null;
                string temppath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp";
                string outputFolder = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                HashSet<string> validNames = new HashSet<string>
                {
                    "append",
                    "assist",
                    "boss",
                    "camera",
                    "campaign",
                    "common",
                    "effect",
                    "enemy",
                    "fighter",
                    "finalsmash",
                    "item",
                    "mihat",
                    "param",
                    "pokemon",
                    "prebuilt",
                    "render",
                    "snapshot",
                    "sound",
                    "spirits",
                    "stage",
                    "standard",
                    "stream",
                    "ui"
                };
                Directory.CreateDirectory(temppath);
                archivePath = mod;
                filename = System.IO.Path.GetFileNameWithoutExtension(archivePath);
                filetype = System.IO.Path.GetExtension(archivePath);
                ParallelLogger.Log($@"[DEBUG] File format {filetype}");
                if (filetype == ".rar")
                {
                    using (ArchiveFile archiveFile = new ArchiveFile(archivePath))
                    {
                        foreach (var fileData in archiveFile.Entries)
                        {
                            if (fileData.FileName.Contains("/") || fileData.FileName.Contains("\\"))
                            {
                                var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                firstDirectoryPath = parts[0].TrimEnd('/', '\\');
                                ParallelLogger.Log($@"[DEBUG] Found first directory {firstDirectoryPath}");
                                break;
                            }
                        }
                        if (validNames.Contains(firstDirectoryPath))
                            extpath = temppath;
                        else
                            extpath = temppath + $@"\" + firstDirectoryPath.ToLower();
                        newpath = outputFolder + $@"\" + filename.ToLower();
                        Directory.CreateDirectory(extpath);
                        archiveFile.Extract(temppath, true);
                    }
                }
                else
                {
                    using (var extractor = new SevenZip.SevenZipExtractor(archivePath))
                    {
                        foreach (var fileData in extractor.ArchiveFileData)
                        {
                            if (fileData.FileName.Contains("/") || fileData.FileName.Contains("\\"))
                            {
                                var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                firstDirectoryPath = parts[0].TrimEnd('/', '\\');
                                ParallelLogger.Log($@"[DEBUG] Found first directory {firstDirectoryPath}");
                                break;
                            }
                        }
                        if (validNames.Contains(firstDirectoryPath))
                            extpath = temppath;
                        else
                            extpath = temppath + $@"\" + firstDirectoryPath.ToLower();
                        newpath = outputFolder + $@"\" + filename.ToLower();
                        Directory.CreateDirectory(extpath);
                        extractor.ExtractArchive(temppath);
                    }
                }
                if (Directory.Exists(extpath))
                {
                    try
                    {
                        if (Directory.Exists(newpath))
                        {
                            bool moved = false;
                            Alert aw = new Alert($@"This will delete {newpath}. Okay?", false);
                            aw.OnAlertHandled = () =>
                            {
                                Directory.Delete(newpath, true);
                                Directory.Move(extpath, newpath);
                                ParallelLogger.Log($@"[DEBUG] Renamed '{extpath}' to '{newpath}'");
                                ParallelLogger.Log($"[INFO] Extracted ''{firstDirectoryPath}''.");
                                moved = true;
                            };
                            aw.ShowDialog();
                            if (moved == false)
                            {
                                Directory.Delete(temppath, true);
                                cancel = true;
                            }
                        }
                        else
                        {
                            Directory.Move(extpath, newpath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ParallelLogger.Log($@"[ERROR] {ex.Message}");
                    }
                }
                if (cancel)
                    return;
                if (System.IO.File.Exists(newpath + $@"\meta.json"))
                {
                    string prev = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2\preview";
                    if (!System.IO.File.Exists(newpath + @"\preview.webp"))
                        await PreviewGet(prev, newpath);
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(newpath + $@"\meta.json");
                    Meta extmod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    extmod.ArchiveImage = true;
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
                if (System.IO.File.Exists(newpath + $@"\info.toml"))
                {
                    Meta extmod = Parser.InfoTOML(newpath + $@"\info.toml");
                    extmod.ID = filename.ToLower();
                    extmod.Link = pagelink;
                    extmod.Type = fromhtml[2];
                    extmod.Category = fromhtml[3];
                    extmod.ArchiveImage = true;
                    string prev = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2\preview";
                    if (!System.IO.File.Exists(newpath + @"\preview.webp"))
                        await PreviewGet(prev, newpath);
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
                else
                {
                    Meta extmod = new Meta();
                    extmod.Name = fromhtml[0];
                    extmod.ID = filename.ToLower();
                    extmod.Link = pagelink;
                    extmod.Type = fromhtml[2];
                    extmod.Category = fromhtml[3];
                    extmod.Description = fromhtml[4];
                    extmod.Authors = authors;
                    extmod.ArchiveImage = true;
                    string prev = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp2\preview";
                    if (!System.IO.File.Exists(newpath + @"\preview.webp"))
                        await PreviewGet(prev, newpath);
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ParallelLogger.Log($"[ERROR] Error during extraction: {ex.Message}");
            }
        }

        public async Task PreviewGet(string prev, string path)
        {
            if (System.IO.File.Exists(prev))
            {
                using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(prev))
                {
                    Directory.CreateDirectory(path);
                    image.Save(path + @"\preview.webp", new WebpEncoder());
                    AemulusModManager.Utilities.ParallelLogger.Log($"[INFO] Image converted to WebP successfully!");
                }
            }
        }
    }
}
