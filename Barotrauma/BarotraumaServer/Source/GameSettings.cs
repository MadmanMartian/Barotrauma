﻿using System.Xml.Linq;

namespace Barotrauma
{
    public enum WindowMode
    {
        Windowed, Fullscreen, BorderlessWindowed
    }

    public partial class GameSettings
    {
        public void Save(string filePath)
        {
            XDocument doc = new XDocument();

            if (doc.Root == null)
            {
                doc.Add(new XElement("config"));
            }

            doc.Root.Add(
                new XAttribute("masterserverurl", MasterServerUrl),
                new XAttribute("autocheckupdates", AutoCheckUpdates),
                new XAttribute("verboselogging", VerboseLogging));

            if (WasGameUpdated)
            {
                doc.Root.Add(new XAttribute("wasgameupdated", true));
            }
            
            if (SelectedContentPackage != null)
            {
                doc.Root.Add(new XElement("contentpackage",
                    new XAttribute("path", SelectedContentPackage.Path)));
            }
            
            doc.Save(filePath);
        }
    }
}
