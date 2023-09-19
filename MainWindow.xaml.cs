using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

namespace ACC_shortcuts
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string macro = @"shortcuts.json";
        public string readme = @"help.txt";
        public string process = "RRRE64"; //ACC
        public JObject jobj;
        // Instantiate a new instance
        KeyboardListener KListener = new KeyboardListener();

        public MainWindow()
        {
            InitializeComponent();
            stop.IsEnabled = false;

            if ((bool)Properties.Settings.Default["onlyAcc"])
            {
                tButton.IsChecked = true;
            }
            else
            {
                tButton.IsChecked = false;
            }

            if (!File.Exists(macro))
            {
                shortcuts.IsEnabled = false;
            }

            if (!File.Exists(readme))
            {
                help.IsEnabled = false;
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            RdWrJsonFile();
            
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown); //start listener
            status.Content = "start: listening";
            stop.IsEnabled = true;
            start.IsEnabled = false;
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            KListener.KeyDown -= new RawKeyEventHandler(KListener_KeyDown); //stop listener
            status.Content = "stop: waiting";
            stop.IsEnabled = false;
            start.IsEnabled = true;
            
        }

        private void Shortcuts(object sender, RoutedEventArgs e)
        {
            if (File.Exists(macro))
            {
                Process.Start(macro); //open file
            }
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            if (File.Exists(readme))
            {
                Process.Start(readme); //open file
            }
        }

        public void RdWrJsonFile()
        {
            string pathInput = macro;
            if (!File.Exists(pathInput))  //if not found, create the file
            {
                using (var sw = new StreamWriter(pathInput))
                {
                    sw.Write("{\"4\": \"Sorry\", \"5\": \"Good race\", \"6\": \"Thanks!\"}");
                    sw.Flush();
                    sw.Close();
                }
            }
            using (StreamReader r = new StreamReader(pathInput))
            {
                var json = r.ReadToEnd();
                jobj = JObject.Parse(json);
            }

        }


        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            // listen only on ACC
            var runningProcessByName = Process.GetProcessesByName(process);
            if (runningProcessByName.Length != 0 || !(bool)Properties.Settings.Default["onlyAcc"])
            {
                string pattern = @"^[a-zA-Z]{1}[\d]{1}$";
                if (Regex.IsMatch(args.Key.ToString(), pattern) || args.Key.ToString().StartsWith("Oem")) 
                {
                    keyboard.Content = args.ToString();
                }
                else
                {
                    keyboard.Content = args.Key.ToString();
                }
                //Console.WriteLine(args.ToString());

                foreach (var item in jobj.Properties())
                {
                    if (String.Equals(args.ToString(), item.Name.ToString(), StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(args.Key.ToString(), item.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        System.Windows.Forms.SendKeys.SendWait("C"); //open chat
                        Thread.Sleep(1000);

                        //print current shorcut
                        System.Windows.Forms.SendKeys.SendWait(item.Value.ToString());
                        Thread.Sleep(1000);

                        System.Windows.Forms.SendKeys.SendWait("{Enter}"); //Enter
                    }
                }
            }
            //Console.WriteLine(args.ToString()); // Prints the text of pressed button

        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)tButton.IsChecked)
            {
                Properties.Settings.Default["onlyAcc"] = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default["onlyAcc"] = true;
                Properties.Settings.Default.Save();
            }
            
        }

    }

}
