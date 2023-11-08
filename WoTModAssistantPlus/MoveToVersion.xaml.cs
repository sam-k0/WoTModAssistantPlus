using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WoTModAssistant
{
    /// <summary>
    /// Interaktionslogik für MoveToVersion.xaml
    /// </summary>
    public partial class MoveToVersion : Window
    {
        private ObservableCollection<GameVersion> gameVersions;
        private ModInfo modInfo;
        private GameVersion currentVersion;

        public MoveToVersion(ObservableCollection<GameVersion> gameVersions, ModInfo modInfo, GameVersion currentVersion)
        {
            InitializeComponent();
            this.gameVersions = new ObservableCollection<GameVersion>(gameVersions.Reverse());
            ListView_VersionSelect.ItemsSource = this.gameVersions;
            ListView_VersionSelect.SelectedIndex = 0; // Select newest
            this.modInfo = modInfo;
            this.currentVersion = currentVersion;
        }

        private void ButtonMove_Click(object sender, RoutedEventArgs e)
        {
             GameVersion selected = (GameVersion)ListView_VersionSelect.SelectedItem;
             if (selected != null)
             {
                string destination = selected.FullPath; // src dir
                string source = currentVersion.FullPath; // dst dir                

                // Check if the mod is already installed in the destination
                if (File.Exists(Path.Combine(destination, modInfo.LocalFileName)))
                {
                    MessageBoxResult result = MessageBox.Show("The mod is already installed in the destination. Do you want to overwrite it?", "Mod already installed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                // Move the mod
                File.Move(Path.Combine(source, modInfo.LocalFileName), Path.Combine(destination, modInfo.LocalFileName));

                MessageBox.Show(string.Format("Success! Moved {0} from {1} ~> {2}", modInfo.ModName, currentVersion.Version, selected.Version));
             }
        }

        private GameVersion resolveGameVersion(object sender)
        {
            Button downloadButton = sender as Button;
            ListViewItem listViewItem = FindAncestor<ListViewItem>(downloadButton);

            if (listViewItem != null)
            {
                GameVersion item = listViewItem.DataContext as GameVersion;
                if (item != null)
                {
                    return item;
                }
            }
            throw new Exception("Could not resolve GameVersion");
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

    }
}
