using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Snapcode
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }
        public static string? LaunchProjectPath { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Any(a => a.IndexOf("run-tests", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                string reportPath = @"c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\view_designer_test_report.md";
                Snapcode.Services.ViewDesignerTestService.RunAllTests(reportPath);
                Environment.Exit(0);
                return;
            }

            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();
            for (int i = 1; i < cmdArgs.Length; i++)
            {
                if (cmdArgs[i] == "--run-tests")
                {
                    string reportPath = @"c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\view_designer_test_report.md";
                    Snapcode.Services.ViewDesignerTestService.RunAllTests(reportPath);
                    Environment.Exit(0);
                    return;
                }
                else if (cmdArgs[i] == "--project" && i + 1 < cmdArgs.Length)
                {
                    LaunchProjectPath = cmdArgs[i + 1];
                    break;
                }
                else if (System.IO.Directory.Exists(cmdArgs[i]))
                {
                    LaunchProjectPath = cmdArgs[i];
                    break;
                }
            }

            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
