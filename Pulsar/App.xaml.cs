using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Debug debugLogger = new Debug();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check if there are command-line arguments
            if (e.Args.Length > 0)
            {
                string url = e.Args[0];
                debugLogger.Log($"Program opened with URL: {url}");

                // Handle the URL if needed
                HandleCustomProtocol(url);
            }
            else
            {
                debugLogger.Log("Program started normally (no URL).");
            }
        }

        private void HandleCustomProtocol(string url)
        {
            // Add your URL handling logic here
            if (url.StartsWith("quasar:"))
            {
                debugLogger.Log("Handling custom protocol: " + url);
                // Additional parsing or handling logic here
            }
            else
            {
                debugLogger.Log("Unexpected URL format.");
            }
        }
    }

}
