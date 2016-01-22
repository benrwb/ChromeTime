using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ChromeTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, int> database;


        public MainWindow()
        {
            database = LoadSettings();

            InitializeComponent();

            // Timer 1
            // Check foreground window, every second
            var tscan = new DispatcherTimer();
            tscan.Interval = TimeSpan.FromSeconds(1);
            tscan.Tick += t_Tick;
            tscan.Start();

            // Timer 2
            // Autosave database every 5 minutes
            // in case app closed abnormally
            // (e.g. power off, force-closed by system)
            var tsave = new DispatcherTimer();
            tsave.Interval = TimeSpan.FromMinutes(5);
            tsave.Tick += SaveTimer_Tick;
            tsave.Start();
        }


        void SaveTimer_Tick(object sender, EventArgs e)
        {
            SaveSettings(database);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings(database);
        }

        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        void t_Tick(object sender, EventArgs e)
        {
            // Prepare database
            var today = DateTime.Now.AddHours(-4).ToShortDateString(); // -4 so that upto 4:00 AM counts as previous day
            if (!database.ContainsKey(today))
                database.Add(today, 0);

            // Get active window & check if it matches
            IntPtr hwnd = GetForegroundWindow();
            string curWndTitle = "";
            if (hwnd != IntPtr.Zero)
            {
                var sb = new StringBuilder(256);
                GetWindowText(hwnd, sb, sb.Capacity);
                curWndTitle = sb.ToString();

                if (curWndTitle.Contains("Google Chrome") 
                    && !curWndTitle.Equals("Videostream - Google Chrome")
                    && !curWndTitle.Equals("Apps - Google Chrome")
                    && !curWndTitle.Equals("New Tab - Google Chrome")
                    )
                    database[today]++;
            }

            // Update UI (if necessary)
            var summary = string.Join("\r\n\r\n", database.Select(z => z.Key + "\t" + TimeSpan.FromSeconds(z.Value).ToString()).Reverse());
            if (summary != textBox1.Text)
                textBox1.Text = summary;

            if (curWndTitle != tbCurWnd.Text)
                tbCurWnd.Text = curWndTitle;
        }



        private static string GetSettingsFileName()
        {
            string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChromeTime");
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            return Path.Combine(folderName, "database.json");
        }

        private static Dictionary<string, int> LoadSettings()
        {
            var fileName = GetSettingsFileName();
            if (File.Exists(fileName))
            {
                // Load previously-saved database
                JavaScriptSerializer ser = new JavaScriptSerializer();
                using (var sr = new StreamReader(fileName))
                {
                    return ser.Deserialize<Dictionary<string, int>>(sr.ReadToEnd());
                }
            }
            else
            {
                // Create new database
                return new Dictionary<string, int>();
            }
        }

        private static void SaveSettings(Dictionary<string, int> database)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = ser.Serialize(database);

            // Save changes to temp file, keep original in case an error occurs whilst saving
            // (sounds unlikely but has happened and left me with 0KB database file)
            var filename = GetSettingsFileName();
            using (StreamWriter sw = new StreamWriter(filename + ".TMP"))
            {
                sw.Write(json);
            }
            // If temp file saved successfully then rename
            File.Delete(filename + ".OLD");
            File.Move(filename, filename + ".OLD");
            File.Move(filename + ".TMP", filename);
        }
    }
}
