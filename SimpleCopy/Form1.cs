﻿using System;
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
            try
            {
                string sourceDirectoryPath = txtSource.Text;
                string destinationDirectoryPath = txtDestination.Text;
                if (string.IsNullOrEmpty(sourceDirectoryPath) || string.IsNullOrEmpty(destinationDirectoryPath))
                {
                    throw new Exception($"Source Directory and Destination Directory cannot be empty");
                }
                var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
                if (!sourceDirectory.Exists)
                {
                    throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory.FullName}");
                }
                var destinationDirectory = new DirectoryInfo(destinationDirectoryPath);
                CleanDestinationDirectory(destinationDirectory);
                progressBar1.Visible = true;
                await Task.Factory.StartNew(() => CopyDirectory(sourceDirectory, destinationDirectory, true));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                progressBar1.Visible = false;
            }
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
            var files = sourceDir.GetFiles();
            Parallel.ForEach(files, file =>
            {
                string targetFilePath = Path.Combine(destinationDir.FullName, file.Name);
                file.CopyTo(targetFilePath);
            });
            
            if (recursive)
            {
                DirectoryInfo[] subDirectories = sourceDir.GetDirectories();
                Parallel.ForEach(subDirectories, subDir =>
                {
                    string newDestinationDir = Path.Combine(destinationDir.FullName, subDir.Name);
                    CopyDirectory(subDir, new DirectoryInfo(newDestinationDir), true);
                });
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
