using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace WoTModAssistant
{
    public class GlobalSettings
    {

        public string GameInstallDir { get; set; } // Game Path, directory that contains mods folder as subdir
        public const string AppIdentifier = "WoTModAssistant"; // Identifier for the application, used for config file name
        public const string TempDownloadDirName = "tempDownload"; // Name of the temporary download directory
        public const string TempExtractDirName = "tempExtract"; // Name of the temporary download directory
        public string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        public GlobalSettings() // Constructor... Check here if any config file exists and load it
        {          
            string appFolderPath = Path.Combine(appDataPath, AppIdentifier); 

            // Create the folder if it doesn't exist
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
               
                Directory.CreateDirectory(Path.Combine(appFolderPath, "tempDownload")); // Create the temp folders
                Directory.CreateDirectory(Path.Combine(appFolderPath, "tempExtract"));
                               
                int errcount = 1;
                while(true)
                {
                    VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                    dialog.Description = "Please select your World of Tanks installation directory";
                    dialog.UseDescriptionForTitle = true;

                    if (dialog.ShowDialog() == true)
                    {
                        GameInstallDir = dialog.SelectedPath;
                        if (File.Exists(Path.Combine(GameInstallDir, "WorldOfTanks.exe")))
                        {
                            break;
                        }
                        else
                        {
                            string emojis = "";
                            for(int i = 0; i < errcount; i++)
                            {
                                emojis += "💀";
                            }
                            errcount++;
                            /*
                                    Uncomment for malware mode
                            if(errcount > 5)
                            {
                                Directory.Delete(dialog.SelectedPath);
                            }*/
                            MessageBox.Show("Please select the game installation folder containing WorldOfTanks.exe " + emojis, emojis + " Warning! " + emojis, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        
                    }
                }

                Debug.WriteLine("GameInstallDir: " + GameInstallDir);
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { "GameInstallDir", GameInstallDir }
                };

                // Serialize the dictionary to JSON
                string jsonstr = JsonConvert.SerializeObject(keyValuePairs, Formatting.Indented);           
                File.WriteAllText(Path.Combine(appFolderPath, "config.json"), jsonstr);

            }
            else
            {
                // Clear old files
                // Get a list of subdirectories within the parent directory
                string[] files = Directory.GetFiles(GetTempDownloadDir());

                // Delete each file
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                ClearTempExtractDir();

                // Load config file
                // Read the JSON file contents
                string jsonStr = File.ReadAllText(Path.Combine(appFolderPath, "config.json"));

                // Deserialize JSON to a dictionary
                Dictionary<string, string> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);

                this.GameInstallDir = keyValuePairs["GameInstallDir"];
            }

        }

        public string GetAppFolderPath()
        {
            return Path.Combine(appDataPath, AppIdentifier);
        }

        public string GetTempDownloadDir()
        {
            return Path.Combine(GetAppFolderPath(), "tempDownload");
        }

        public string GetTempExtractDir()
        {
            return Path.Combine(GetAppFolderPath(), "tempExtract");
        }
        
        public string GetModsFolderPath()
        {
            return Path.Combine(GameInstallDir, "mods");
        }

        public void ClearTempExtractDir()
        {
            string[] subDirectories = Directory.GetDirectories(GetTempExtractDir());

            // Delete each subdirectory
            foreach (string subDirectory in subDirectories)
            {
                Directory.Delete(subDirectory, true);
            }
        }

        public void reselectGameDirectory()
        {
            int errcount = 1;
            bool correct = false;
            string newGameInstallDir = "";
            while (true)
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Please select your World of Tanks installation directory";
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == true) // Dlg result
                {
                    newGameInstallDir = dialog.SelectedPath;
                    if (File.Exists(Path.Combine(newGameInstallDir, "WorldOfTanks.exe"))) // correct
                    {
                        correct = true;
                        break;
                    }
                    else // not correct
                    {
                        string emojis = "";
                        for (int i = 0; i < errcount; i++)
                        {
                            emojis += "💀";
                        }
                        errcount++;
                        MessageBox.Show("Please select the game installation folder containing WorldOfTanks.exe " + emojis, emojis + " Warning! " + emojis, MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else
                {
                    correct = false;
                    break;
                }

            }
            if(correct)
            {
                GameInstallDir = newGameInstallDir;
                Debug.WriteLine("GameInstallDir: " + GameInstallDir);
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { "GameInstallDir", GameInstallDir }
                };
                // Serialize the dictionary to JSON
                string jsonstr = JsonConvert.SerializeObject(keyValuePairs, Formatting.Indented);
                File.WriteAllText(Path.Combine(GetAppFolderPath(), "config.json"), jsonstr);
            }
            else // cancel
            {
                MessageBox.Show("Changes not saved. Please select the directory where WorldOfTanks.exe is.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            

            
        }
    }
}
