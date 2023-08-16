using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoTModAssistant
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ModInfo> Mods { get; } = new ObservableCollection<ModInfo>();
        public ObservableCollection<ModInfo> InstalledMods { get; } = new ObservableCollection<ModInfo>();
        public ObservableCollection<ModInfo> RecommendedMods { get; } = new ObservableCollection<ModInfo>();

        public GlobalSettings Settings { get; } = new GlobalSettings();

        public MainWindow()
        {
            InitializeComponent();            

            // Set the Mods collection as the ItemsSource for the ListView
            ListView_SearchBrowseMods.ItemsSource = Mods;
            ListView_InstalledMods.ItemsSource = InstalledMods;
        }

        // Hyperlink click event handler
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        // Download button click event handler
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            /*Button downloadButton = sender as Button;
            ListViewItem listViewItem = FindAncestor<ListViewItem>(downloadButton);

            if (listViewItem != null)
            {
                ModInfo item = listViewItem.DataContext as ModInfo;
                if (item != null)
                {
                    Debug.WriteLine("Downloading " + item.ModName);

                    ModDownload downloader = new ModDownload(item.DownloadUri, Settings, downloadButton);
                    string[] urlparts = item.DownloadUri.Split('.');
                    string fileextension = "."+urlparts[urlparts.Length - 1];
                    string localfilename = item.ModID + fileextension;
                    downloader.StartDownload(localfilename);
                }
            }*/

            Button downloadButton = sender as Button;
            ModInfo item = resolveModInfo(sender);
            Debug.WriteLine("Downloading " + item.ModName);

            ModDownload downloader = new ModDownload(item.DownloadUri, Settings, downloadButton);
            string[] urlparts = item.DownloadUri.Split('.');
            string fileextension = "." + urlparts[urlparts.Length - 1];
            string localfilename = item.ModID + fileextension;
            downloader.StartDownload(localfilename);
        }


        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }

                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);

            return null;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ModWebRequest webRequester = new ModWebRequest();
            var data = Task.Run(() => webRequester.GetModsByKeyword("xvm"));
            data.Wait();
            // Auswerten der Response
            string response = data.Result;
            // Print to console
            Debug.WriteLine(response);


        }

        private void SearchRecommendedButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Search recommended button clicked");
            ModWebRequest webRequester = new ModWebRequest();
            var data = Task.Run(() => webRequester.GetRecommendedMods());
            data.Wait();
            // Auswerten der Response
            string response = data.Result;
            
            // Convert to JSON
            JObject json = JObject.Parse(response);
            int count = (int)json["recommended"]["count"];

            Debug.WriteLine("Count: " + count);
           
            // Set the count label
            Label_RecommendedCnt.Content = count.ToString();

            // Get the list
            JArray list = (JArray)json["recommended"]["results"];
            ObservableCollection<ModInfo> RecommendedMods = new ObservableCollection<ModInfo>();

            foreach (JObject modobj in list)
            {
                Debug.WriteLine("----------------------");
                JArray localizations = (JArray)modobj["localizations"];

                int english = resolveLanguage(localizations);
               

                string title = (string)modobj["localizations"][english]["title"];
                string description = (string)modobj["localizations"][english]["description"];
                string installationGuide = (string)modobj["localizations"][english]["installation_guide"];
                string version = (string)modobj["versions"][0]["version"]; // 0 for newest version
                string downloadUrl = (string)modobj["versions"][0]["download_url"];

                Debug.WriteLine("Title: " + title);
                Debug.WriteLine("Description: " + description);
                Debug.WriteLine("Installation Guide: " + installationGuide);
                Debug.WriteLine("Version: " + version);
                Debug.WriteLine("Download URL: " + downloadUrl);

                // create new ModInfo object
                ModInfo mod = new ModInfo();
                mod.ModName = title;
                mod.ModID = (string)modobj["id"];
                mod.Author = (string)modobj["owner"]["spa_username"];
                mod.Version = version;
                //mod.Description = description;
                mod.WebpageUri = new Uri("https://wgmods.net/" + mod.ModID);
                mod.DownloadUri = downloadUrl;

                RecommendedMods.Add(mod);
            }

            modListViewRecommended.ItemsSource = RecommendedMods;
        }

        // Scheiss russische trollzeichen
        private int resolveLanguage(JArray localizations)
        {
            int langCount = localizations.Count; // can be either 1 or 2 (Russian and English)
            
            // Loop over all languages
            for(int i = 0; i < langCount; i++)
            {
                string language = (string)localizations[i]["lang"]["code"];
                if(language == "en")
                {
                    return i;
                }
            }
            return 0;
        }

        /*
            Installed section
        */
        // Refresh the Combobox and set the newest version as selected
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.ClearTempExtractDir();
            ObservableCollection<string> items = GetGameVersionFolders();

            ComboBox_Version.ItemsSource = items;
            ComboBox_Version.SelectedIndex = items.Count - 1; // Set to newest version

        }

        private ObservableCollection<string> GetGameVersionFolders()
        {
            ObservableCollection<string> items = new ObservableCollection<string>();

            foreach (string subdirectory in Directory.EnumerateDirectories(Settings.GetModsFolderPath()))
            {
                if (int.TryParse(subdirectory.Replace(Settings.GetModsFolderPath() + "\\", "").Replace(".", ""), out int versionNumber))
                {
                    items.Add(subdirectory);
                }
                Debug.WriteLine(subdirectory.Replace(Settings.GetModsFolderPath() + "\\", "").Replace(".", ""));
            }

            return items;
        }

        private void ComboBox_Version_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0)
            {
                return;
            }
            string selectedVersionDir = e.AddedItems[0].ToString();
            Debug.WriteLine("Selected version: " + selectedVersionDir);

            // Get the mods for the selected version
            ObservableCollection<ModInfo> InstalledMods = new ObservableCollection<ModInfo>();

            string[] filepaths = Directory.GetFiles(selectedVersionDir)
                                     .Where(file => file.EndsWith(".wotmod", StringComparison.OrdinalIgnoreCase) ||
                                                    file.EndsWith(".wotmod.off", StringComparison.OrdinalIgnoreCase))
                                     .Select(Path.GetFullPath)
                                     .ToArray();

            foreach (string filepath in filepaths)
            {
                Debug.WriteLine("Filename: " + filepath);
                string destinationDirectory;
                using (ZipArchive archive = ZipFile.OpenRead(filepath))
                {
                    // Combine the versioned folder name with the extraction directory
                    destinationDirectory= Path.Combine(Settings.GetTempExtractDir(), HashModFilename(filepath,8));
                    //Debug.WriteLine("Extracting to: " + destinationDirectory);
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(destinationDirectory);
                    archive.ExtractToDirectory(destinationDirectory);                   
                }

                // read the meta.xml file
                bool isActive = (Path.GetExtension(filepath) == ".wotmod");
                string localfilename = filepath.Replace(selectedVersionDir + "\\", "");

                if (File.Exists(Path.Combine(destinationDirectory, "meta.xml")))
                {
                    InstalledMods.Add(new ModInfo(File.ReadAllText(Path.Combine(destinationDirectory, "meta.xml")), isActive, localfilename));
                }
                else
                {
                    ModInfo modInfo = new ModInfo();
                    modInfo.IsEnabled = isActive;
                    modInfo.LocalFileName = localfilename;
                    modInfo.Author = localfilename;
                    InstalledMods.Add(modInfo);
                }
               
               
            }
                
            // Update the data binding
            ListView_InstalledMods.ItemsSource = InstalledMods;

        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // the listbox button
            ModInfo modInfo = resolveModInfo(sender); // get the mod info from the button
            string selectedVersionDir = ComboBox_Version.SelectedItem.ToString(); // get the selected version

            // Dlg
            MessageBoxResult result = MessageBox.Show(string.Format("Do you really want to uninstall {0}?", modInfo.ModName), "Uninstall", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) // If yes, delete the file
            {
                File.Delete(Path.Combine(selectedVersionDir, modInfo.LocalFileName));
            }
        }

        private void ToggleEnableButton_Click(object sender, RoutedEventArgs e)
        {
            // Initial button text is set by the Actvate/Deactivate Binding in the XAML
            // Handle deactivate button click here
            Button button = sender as Button;
            ModInfo modInfo = resolveModInfo(sender);
            string selectedVersionDir = ComboBox_Version.SelectedItem.ToString();


            Debug.WriteLine("Toggle button clicked for " + modInfo.ModName + " Local File Name: "+modInfo.LocalFileName + " Selected: " + selectedVersionDir);

            modInfo.IsEnabled = !modInfo.IsEnabled;
            // Rename the modfile
            string newFilename = GetModfileEnabledDisabled(modInfo.LocalFileName, modInfo.IsEnabled);
            File.Move(Path.Combine(selectedVersionDir, modInfo.LocalFileName), Path.Combine(selectedVersionDir, newFilename));

            modInfo.LocalFileName = newFilename;

            switch (modInfo.IsEnabled)
            {
                case true: // Activate
                    button.Content = "Deactivate";                    
                    break;
                case false: // Deactivate
                    button.Content = "Activate";
                    break;
            }
        }

        private void MoveVersionButton_Click(object sender, RoutedEventArgs e)
        {
            ModInfo modInfo = resolveModInfo(sender);
            string selectedVersionDir = ComboBox_Version.SelectedItem.ToString();

            // Get the game versions and make a list of GameVersion objects
            ObservableCollection<GameVersion> gameVersions = new ObservableCollection<GameVersion>();
            foreach (string versionDir in GetGameVersionFolders())
            {
                gameVersions.Add(new GameVersion(versionDir, versionDir.Replace(Settings.GetModsFolderPath() + "\\", "")));
            }
            
            MoveToVersion moveToVersion = new MoveToVersion(gameVersions, modInfo,
                            new GameVersion(selectedVersionDir, selectedVersionDir.Replace(Settings.GetModsFolderPath() + "\\", "")));
            
            moveToVersion.ShowDialog();
        }

        private ModInfo resolveModInfo(object sender)
        {
            Button downloadButton = sender as Button;
            ListViewItem listViewItem = FindAncestor<ListViewItem>(downloadButton);

            if (listViewItem != null)
            {
                ModInfo item = listViewItem.DataContext as ModInfo;
                if (item != null)
                {
                    return item;
                }
            }
            throw new Exception("Could not resolve ModInfo");
        }

        private string GetModfileEnabledDisabled(string fname, bool enabled)
        {
            if(enabled) // Enable mod
            {
                if (fname.EndsWith(".wotmod.off"))
                {
                    return fname.Replace(".wotmod.off", ".wotmod");
                }
            }
            
            if(!enabled) // Disable mod
            {
                if (fname.EndsWith(".wotmod"))
                {
                    return fname.Replace(".wotmod", ".wotmod.off");
                }
            }
            
            return fname; // already correct?
            
            
        }

        static string HashModFilename(string input, int length)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Take the first 'length' bytes of the hash and convert to hexadecimal
                string truncatedHash = BitConverter.ToString(hashBytes, 0, length).Replace("-", "");

                return truncatedHash;
            }
        }
    }
}
