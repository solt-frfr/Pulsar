using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AemulusModManager;
using AemulusModManager.Utilities;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool isNewInstance;
            _mutex = new Mutex(true, "MyUniqueAppName", out isNewInstance);
            bool oneClick = e.Args.Length > 0;
            if (isNewInstance)
            {
                MainWindow mw = new MainWindow(false, oneClick);
                mw.Show();
                if (!oneClick)
                {
                    ParallelLogger.Log($@"[DEBUG] Pulsar opened normally.");
                }
            }
            if (e.Args.Length > 0)
            {
                HandleCustomProtocol(e.Args[0]);
            }
            if (!isNewInstance)
            {
                Shutdown();
            }
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        private void HandleCustomProtocol(string url)
        {
            if (url.StartsWith("quasar:"))
            {
                ParallelLogger.Log($@"[DEBUG] Handling custom protocol: {url}");
                Alert aw = new Alert($@"Handling custom protocol: {url}");
                aw.ShowDialog();
            }
            else
            {
                ParallelLogger.Log("[DEBUG] Unexpected URL format.");
            }
        }
    }

}
