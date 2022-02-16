﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleCopy
{
    public partial class Form1 : Form
    {
        private static bool cleanDirectory = true;
        private string[] ignoredFolders = { ".git", "node_modules" };
        public Form1()
        {
            InitializeComponent();
        }

        private async void ButtonCopy_Click(object sender, EventArgs e)
        {
            string sourceDirectoryPath = txtSource.Text;
            string destinationDirectoryPath = txtDestination.Text;

            var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory.FullName}");
            }
            var destinationDirectory = new DirectoryInfo(destinationDirectoryPath);
            CleanDestinationDirectory(destinationDirectory);
            progressBar1.Visible = true;
            await Task.Factory.StartNew(() => CopyDirectory(sourceDirectory, destinationDirectory, true));
            progressBar1.Visible = false;
        }

        private void CopyDirectory(DirectoryInfo sourceDir, DirectoryInfo destinationDir, bool recursive)
        {
            if (IsDirectoryIgnored(sourceDir))
            {
                return;
            }
            if (!destinationDir.Exists)
            {
                Directory.CreateDirectory(destinationDir.FullName);
            }

            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir.FullName, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                DirectoryInfo[] subDirectories = sourceDir.GetDirectories();
                foreach (DirectoryInfo subDir in subDirectories)
                {
                    string newDestinationDir = Path.Combine(destinationDir.FullName, subDir.Name);

                    CopyDirectory(subDir, new DirectoryInfo(newDestinationDir), true);
                }
            }
        }

        private static void CleanDestinationDirectory(DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                if (cleanDirectory)
                {
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in directory.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"Directory existed: {directory.FullName}");
                }
            }
        }

        private bool IsDirectoryIgnored(DirectoryInfo directory)
        {
            return ignoredFolders.Any(x => x == directory.Name.ToLower());
        }
    }
}
