﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ChromeTime
{
    class MainMain
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        const int SW_SHOW = 5;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        //[System.Diagnostics.DebuggerNonUserCodeAttribute()] // commented out to enable debugging
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            Process currentProcess = Process.GetCurrentProcess();
            var runningProcess = (from process in Process.GetProcesses()
                                  where
                                    process.Id != currentProcess.Id &&
                                    process.ProcessName.Equals(currentProcess.ProcessName, StringComparison.Ordinal)
                                  select process).FirstOrDefault();



            if (runningProcess != null)
            {
                // App is already running, bring to front
                // Note: Will not work if ShowInTaskbar="False"
                ShowWindow(runningProcess.MainWindowHandle, SW_SHOW); // restore if minimised
                SetForegroundWindow(runningProcess.MainWindowHandle); // activate and bring to front
            }
            else
            {
                // App is not already running, show the main window
                ChromeTime.App app = new ChromeTime.App();
                app.InitializeComponent();
                app.Run();
            }
        }




    }
}
