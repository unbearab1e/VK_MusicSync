using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Net;
using System.IO;
using Ookii.Dialogs.Wpf; 

namespace VKMusicSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Elysium.Controls.Window
    {
        VKAPI VK = ((App)Application.Current).VK;
        List<VKAPI.Audio> downloadQueue = new List<VKAPI.Audio>();
        String selectedFolder = "";
        Int32 itemsInQueue = 0;
        StringBuilder newlyAdded = new StringBuilder();
        String[] fileContent = new String[2];

        public MainWindow()
        {
            for (Int32 i = 0; i < fileContent.Length; i++) fileContent[i] = "";
            InitializeComponent();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VK.OAuth() == true)
                {
                    ((Control)sender).IsEnabled = false;
                    this.ChooseFolderButton.IsEnabled = true;
                    String[] myName = VK.Users_get_name(Convert.ToInt32(VK.UserID));
                    this.LogInButton.Content = myName[0] + " " + myName[1];

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Occured");
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Int32[] alreadyDownloaded = { };
                if (File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, VK.UserID + ".dat")))
                {
                    fileContent = File.ReadAllLines(System.IO.Path.Combine(Environment.CurrentDirectory, VK.UserID + ".dat"));
                    if (fileContent.Length == 2 && fileContent[0] == VK.UserID.ToString())
                        alreadyDownloaded = fileContent[1].Split(',').Select(x => int.Parse(x)).ToArray();
                    else fileContent = new String[2];
                }
                List<VKAPI.Audio> audios = VK.Audio_get_all(Convert.ToInt32(VK.UserID));

                newlyAdded = new StringBuilder();
                this.downloadQueue = new List<VKAPI.Audio>();

                Int32 counter = 0;

                foreach (VKAPI.Audio track in audios)
                {
                    if (alreadyDownloaded.Count() != 0 && alreadyDownloaded.Contains(track.aid)) continue;
                    this.downloadQueue.Add(track);
                    counter++;
                }

                if (downloadQueue.Count != 0)
                {
                    itemsInQueue = downloadQueue.Count;
                    this.DownloadingProgressBar.Visibility = System.Windows.Visibility.Visible;
                    this.DownloadingProgressBar.Maximum = downloadQueue.Count;
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    this.TaskbarItemInfo.ProgressValue = 0;
                    DownloadAudio();
                }
                else
                {
                    itemsInQueue = 0;
                    this.StatusMessageLabel.Content = "No new music found";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Occured");
            }
        }

        private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.selectedFolder = "";
                VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
                folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                folderDialog.ShowNewFolderButton = true;
                if (folderDialog.ShowDialog() == true)
                {
                    selectedFolder = folderDialog.SelectedPath;
                    ((Control)sender).IsEnabled = false;
                    this.DownloadButton.IsEnabled = true;
                    this.ChooseFolderButton.Content = selectedFolder;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Occured");
            }
        }

        private void DownloadAudio()
        {
            try
            {
                if (this.downloadQueue.Count == 0)
                {
                    this.DownloadingProgressBar.Visibility = System.Windows.Visibility.Hidden;
                    this.StatusMessageLabel.Content = itemsInQueue + " new files added";
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    this.fileContent[0] = VK.UserID.ToString();
                    if (!String.IsNullOrEmpty(fileContent[1])) fileContent[1] += "," + newlyAdded.ToString();
                    else fileContent[1] = newlyAdded.ToString();
                    this.fileContent[1] = fileContent[1].Substring(0, fileContent[1].Length - 1);
                    File.WriteAllLines(System.IO.Path.Combine(Environment.CurrentDirectory, VK.UserID + ".dat"), fileContent);
                    return;
                }
                VKAPI.Audio track = this.downloadQueue[0];
                this.StatusMessageLabel.Content = "[" + (itemsInQueue - downloadQueue.Count + 1) + "/" + itemsInQueue + "] Downloading: " + track.artist + " - " + track.title;
                String filename = this.selectedFolder + "\\" + track.artist + " - " + track.title + ".mp3";
                WebClient wc = new WebClient();
                wc.DownloadFileAsync(new Uri(track.url), filename);
                wc.DownloadFileCompleted += ((object sender, System.ComponentModel.AsyncCompletedEventArgs e) => { DownloadAudio(); });
                this.downloadQueue.Remove(track);
                this.DownloadingProgressBar.Value = itemsInQueue - downloadQueue.Count;
                this.TaskbarItemInfo.ProgressValue = (double)(itemsInQueue - downloadQueue.Count) / (double)itemsInQueue;
                newlyAdded.Append(track.aid + ",");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Exception Occured");
            }
        }
    }
}
