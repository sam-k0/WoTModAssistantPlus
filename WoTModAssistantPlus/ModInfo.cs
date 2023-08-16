using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WoTModAssistant
{
    public class ModInfo
    {
        public string ModName { get; set; } = "unknown";
        public string ModID { get; set; } = "unknown";
        public string Author { get; set; } = "unknown";
        public string Version { get; set; } = "0.0";
        public string Description { get; set; } = "";
        public double Rating { get; set; } = 0.0;
        public Uri WebpageUri { get; set; } = new Uri("http://www.wgmods.net");
        public string DownloadUri { get; set; } = "http://www.wgmods.net";
        public string LocalFileName { get; set; } // can be null , this is ONLY the filename, not the full path
        public bool IsEnabled { get; set; } = false; // For browse results this is obv false

        public ModInfo()
        {
        }

        public ModInfo(string modName, string modID, string author, string version, string description, double rating, Uri webpageUri, string downloadUri, string localFileName)
        {
            ModName = modName;
            ModID = modID;
            Author = author;
            Version = version;
            Description = description;
            Rating = rating;
            WebpageUri = webpageUri;
            DownloadUri = downloadUri;
            LocalFileName = localFileName;
        }

        public ModInfo(string modName, string modID, string author, string version,
            string description, double rating, Uri webpageUri, string downloadUri, string localFileName, bool isEnabled)
        {
            ModName = modName;
            ModID = modID;
            Author = author;
            Version = version;
            Description = description;
            Rating = rating;
            WebpageUri = webpageUri;
            DownloadUri = downloadUri;
            LocalFileName = localFileName;
            IsEnabled = isEnabled;
        }

        public ModInfo(string xmlstr, bool isEnabled, string localFileName)
        {
            Dictionary<string, string> xmlDict = new Dictionary<string, string>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlstr);

            XmlNode root = xmlDoc.DocumentElement;
            if (root != null)
            {
                foreach (XmlNode node in root.ChildNodes)
                {
                    xmlDict[node.Name] = node.InnerText;
                }
            }

            // Bruh not all keys always exist
            if(xmlKeyExists(xmlDoc, "name"))
            {
                ModName = xmlDict["name"];
            }

            if (xmlKeyExists(xmlDoc, "version"))
            {
                Version = xmlDict["version"];
            }

            if (xmlKeyExists(xmlDoc, "id"))
            {
                Author = xmlDict["id"];
            }

            if (xmlKeyExists(xmlDoc, "description"))
            {
                Description = xmlDict["description"];
            }

            IsEnabled = isEnabled;
            LocalFileName = localFileName;

        }

        private bool xmlKeyExists(XmlDocument xmlDoc, string keyToCheck)
        {
            XmlNode node = xmlDoc.SelectSingleNode("//" + keyToCheck);

            return (node != null);
        }
    }
}
