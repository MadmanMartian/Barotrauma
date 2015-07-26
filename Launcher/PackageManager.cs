﻿using Subsurface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class PackageManager : Form
    {
        private ContentPackage selectedPackage;

        private List<ListBox> fileBoxes;
        private List<Button> fileButtons;

        public PackageManager(ContentPackage _selectedPackage)
        {
            InitializeComponent();

            packageList.DisplayMember = "Name";
            packageList.ValueMember = "MD5hash";

            foreach (ContentPackage package in ContentPackage.list)
            {
                packageList.Items.Add(package); 
            }

            fileBoxes   = new List<ListBox>();
            fileButtons = new List<Button>();

            fileBoxes.Add(itemList);
            itemList.Tag        = ContentType.Item;
            itemButton.Tag      = ContentType.Item;
            itemFolder.Tag      = ContentType.Item;
            fileButtons.Add(itemButton);
            fileButtons.Add(itemFolder);

            fileBoxes.Add(characterList);
            characterList.Tag   = ContentType.Character;
            characterButton.Tag = ContentType.Character;
            characterFolder.Tag = ContentType.Character;
            fileButtons.Add(characterButton);
            fileButtons.Add(characterFolder);

            fileBoxes.Add(structureList);
            structureList.Tag   = ContentType.Structure;
            structureButton.Tag = ContentType.Structure;
            structureFolder.Tag = ContentType.Structure;
            fileButtons.Add(structureButton);
            fileButtons.Add(structureFolder);

            fileBoxes.Add(jobList);
            jobList.Tag         = ContentType.Jobs;
            jobButton.Tag       = ContentType.Jobs;
            jobFolder.Tag       = ContentType.Jobs;
            fileButtons.Add(jobButton);
            fileButtons.Add(jobFolder);

            foreach (Button fileButton in fileButtons)
            {
                fileButton.Enabled = false;
            }

            selectedPackage = _selectedPackage;
            SelectPackage(selectedPackage);
        }

        private void packageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox listBox = sender as ListBox;

            SelectPackage(listBox.SelectedItem as ContentPackage);
        }

        private void SelectPackage(ContentPackage package)
        {
            selectedPackage = package;

            foreach (Button fileButton in fileButtons)
            {
                fileButton.Enabled = (selectedPackage != null);
            }

            if (selectedPackage == null)
            {
                foreach (ListBox fileBox in fileBoxes)
                {
                    fileBox.Items.Clear();
                }
            }
            else
            {

                foreach (ListBox fileBox in fileBoxes)
                {
                    ContentType type = (fileBox.Tag is ContentType) ? (ContentType)fileBox.Tag : ContentType.None;

                    foreach (ContentFile file in selectedPackage.files)
                    {
                        if (file.type != type) continue;

                        fileBox.Items.Add(file);
                    }
                }
            }
        }

        private void newPackage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(newPackageName.Text)) return;
            ContentPackage newPackage = ContentPackage.CreatePackage(newPackageName.Text);

            packageList.Items.Add(newPackage);
            packageList.SelectedItem = newPackage;

            newPackageName.Text = "";
        }

        private void newPackageName_TextChanged(object sender, EventArgs e)
        {
            newPackage.Enabled = !string.IsNullOrEmpty(newPackageName.Text);
        }

        private void addFileButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            ContentType type = (button.Tag is ContentType) ? (ContentType)button.Tag : ContentType.None;
            Debug.Assert(type != ContentType.None, "ContentType of a button tag was ContentType.None");

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML files (*.xml)|*.xml;*.XML";
            //ofd.RestoreDirectory?

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] fileNames = ofd.FileNames;

                foreach (string file in fileNames)
                {
                    AddFile(type, file);
                }
            }
        }

        private void AddFile(ContentType type, string path)
        {
            ListBox selectedBox = null;
            foreach (ListBox fileBox in fileBoxes)
            {
                if (type != ((fileBox.Tag is ContentType) ? (ContentType)fileBox.Tag : ContentType.None)) continue;

                selectedBox = fileBox;
                break;
            }

            ContentFile newPackage = selectedPackage.AddFile(GetRelativePath(path, Directory.GetCurrentDirectory()), type);

            if (newPackage!=null) selectedBox.Items.Add(newPackage);
                
        }

        private void addFolderButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            ContentType type = (button.Tag is ContentType) ? (ContentType)button.Tag : ContentType.None;
            Debug.Assert(type != ContentType.None, "ContentType of a button tag was ContentType.None");

            FolderBrowserDialog fbd = new FolderBrowserDialog();

            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "XML files (*.xml)|*.xml;*.XML";
            //ofd.RestoreDirectory?

            if (fbd.ShowDialog() == DialogResult.OK)
            {
               AddFilesInFolder(type, fbd.SelectedPath);

            }
        }

        private void AddFilesInFolder(ContentType type, string folder, string searchPattern ="*xml")
        {
            if (!Directory.Exists(folder)) return;

            string[] files = Directory.GetFiles(folder, "*.xml");

            foreach (string filePath in files)
            {
                AddFile(type, filePath);
            }

            string[] subDirectories = Directory.GetDirectories(folder, "*");
            foreach (string subDir in subDirectories)
            {
                AddFilesInFolder(type, subDir, searchPattern);
            }
        }

        string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (selectedPackage!=null) selectedPackage.Save(LauncherMain.ContentPackageFolder);

            this.Close();
        }
        
    }
}