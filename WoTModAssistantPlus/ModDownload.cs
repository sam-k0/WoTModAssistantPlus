using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace WoTModAssistant
{
    public class ModDownload
    {
        private WebClient webClient;
        private Button downloadButton;
        private string url;
        private string localfilename;

        private GlobalSettings Settings = null;


        public ModDownload(string url, GlobalSettings settings, Button downloadButton)
        {
            this.downloadButton = downloadButton;
            this.url = url;
            this.Settings = settings;

            webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
        }

        public void StartDownload(string savefilename)
        {
            webClient.DownloadFileAsync(new Uri(this.url), this.Settings.GetTempDownloadDir() + "\\" + savefilename);
            localfilename = savefilename; // For moving the file later
            downloadButton.IsEnabled = false;
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadButton.Content = e.ProgressPercentage.ToString() + "%";
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download completed!");
            downloadButton.Content = "Complete";
            // Extract the downloaded file
            unzip();

        }

        private void unzip()
        {
            string localfilepath = this.Settings.GetTempDownloadDir() + "\\" + localfilename;
            string extractdir = this.Settings.GetTempExtractDir();
            string dirname = Path.GetFileNameWithoutExtension(localfilename);

            using (ZipArchive archive = ZipFile.OpenRead(localfilepath))
            {
                // Combine the versioned folder name with the extraction directory
                string destinationDirectory = Path.Combine(extractdir, dirname);
                Debug.WriteLine("Extracting to: " + destinationDirectory);
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(destinationDirectory);

                archive.ExtractToDirectory(destinationDirectory);

                Debug.WriteLine("Extraction completed.");
            }
        }
    }
}
