using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
            if (e.Args.Length > 0)
            {
                string url = e.Args[0];
                debugLogger.Log($"Program opened with URL: {url}");
                HandleCustomProtocol(url);
            }
            else
            {
                debugLogger.Log("Program started normally (no URL).");
            }
            const string appId = "Pulsar.Solt11";
            bool createdNew;
            _mutex = new Mutex(true, appId, out createdNew);

            if (!createdNew)
            {
                HandleRunningInstance(e);
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }
        private void HandleRunningInstance(StartupEventArgs e)
        {
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (process.Id != currentProcess.Id)
                {
                    IntPtr hWnd = process.MainWindowHandle;
                    ShowWindow(hWnd, 5);
                    SetForegroundWindow(hWnd);
                    TriggerFunctionInExistingInstance(e.Args);
                    break;
                }
            }
        }
        private void TriggerFunctionInExistingInstance(string[] args)
        {
            if (args.Length > 0)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Dispatcher.Invoke(() => mainWindow.UpdateLog());
                }
            }
        }
        private void HandleCustomProtocol(string url)
        {
            if (url.StartsWith("quasar:"))
            {
                debugLogger.Log("Handling custom protocol: " + url);
            }
            else
            {
                debugLogger.Log("Unexpected URL format.");
            }
        }
        private static Mutex _mutex;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }

}
