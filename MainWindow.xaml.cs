using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DSXGameHelperv1
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<GameInfo> gamePaths;
        private Timer processCheckTimer;
        private int checkInterval;
        private const string SettingsFilePath = "settings.json";
        private Settings appSettings;

        public string dsxExecutablePath { get; private set; }
        private TaskbarIcon taskbarIcon;


        public MainWindow()
        {
            InitializeComponent();
            appSettings = LoadSettings();
            gamePaths = new ObservableCollection<GameInfo>(appSettings.GamePaths);
            lvGames.ItemsSource = gamePaths;

            DataContext = appSettings;
            InitializeTimer();
            UpdateStatus("Ready. No game running.");


            taskbarIcon = new TaskbarIcon();
            taskbarIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/controller.ico")); // Replace with your own icon path
            taskbarIcon.ToolTipText = "DSX Game Helper";
            taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_DoubleClick;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem openMenuItem = new MenuItem() { Header = "Open" };
            openMenuItem.Click += (sender, e) => OpenMainWindow(sender);
            MenuItem exitMenuItem = new MenuItem() { Header = "Exit" };
            exitMenuItem.Click += (sender, e) => Close();
            contextMenu.Items.Add(openMenuItem);
            contextMenu.Items.Add(exitMenuItem);

            taskbarIcon.ContextMenu = contextMenu;

        }

        private void TaskbarIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            OpenMainWindow(sender);
        }

        private void OpenMainWindow(object sender)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }


        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide(); // Hide the main window
            }
        }


        private void InitializeTimer()
        {
            checkInterval = appSettings.CheckInterval * 1000; // Convert to milliseconds
            processCheckTimer = new Timer(CheckRunningGames, null, 0, checkInterval);
        }

        private Settings LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string settingsJson = File.ReadAllText(SettingsFilePath);
                Settings settings = JsonSerializer.Deserialize<Settings>(settingsJson);

                // Set dsxExecutablePath from the loaded settings
                dsxExecutablePath = settings.DSXExecutablePath;

                return settings ?? new Settings();
            }

            return new Settings();
        }

        private void SaveSettings()
        {
            appSettings.GamePaths = gamePaths.ToList();


            //if (int.TryParse(cbCheckInterval.SelectedItem?.ToString(), out int checkIntervalValue))
            //{
            //    appSettings.CheckInterval = checkIntervalValue;
            //}
            //else
            //{
            //    // Handle the case where parsing fails, e.g., display an error message.
            //    UpdateStatus("Invalid check interval selected.");
            //}

            string settingsJson = JsonSerializer.Serialize(appSettings);
            File.WriteAllText(SettingsFilePath, settingsJson);
        }




        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() => txtStatus.Text = message);
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                InitialDirectory = appSettings.LastUsedDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                appSettings.LastUsedDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                var gameInfo = new GameInfo
                {
                    GamePath = openFileDialog.FileName,
                    GameName = Path.GetFileNameWithoutExtension(openFileDialog.FileName)
                };

                gamePaths.Add(gameInfo);
                SaveSettings();
                UpdateStatus($"Game added: {gameInfo.GameName}");
            }
        }

        private void btnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            if (lvGames.SelectedItem is GameInfo selectedGame)
            {
                gamePaths.Remove(selectedGame);
                SaveSettings();
                UpdateStatus($"Game removed: {selectedGame.GameName}");
            }
            else
            {
                MessageBox.Show("Please select a game to remove.", "No Game Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void CheckRunningGames(object state)
        {
            var runningProcessNames = Process.GetProcesses().Select(p => p.ProcessName).ToList();
            bool anyGameRunning = gamePaths.Any(gameInfo => runningProcessNames.Contains(gameInfo.GameName));

            Dispatcher.Invoke(() =>
            {
                if (anyGameRunning)
                {
                    EnsureDSXIsRunning();
                }
                else
                {
                    EnsureDSXIsNotRunning();
                    UpdateStatus("No game running.");
                }
            });
        }

        private void EnsureDSXIsRunning()
        {
            string dsxProcessName = Path.GetFileNameWithoutExtension(appSettings.DSXExecutablePath);
            if (!IsProcessRunning(dsxProcessName))
            {
                StartDSX();
            }
        }

        private void StartDSX()
        {
            if (string.IsNullOrWhiteSpace(appSettings.DSXExecutablePath))
            {
                UpdateStatus("Game running but no DSX path selected.");
                return;
            }

            if (!File.Exists(appSettings.DSXExecutablePath))
            {
                UpdateStatus("Invalid DSX Path.");
                return;
            }

            if (appSettings.DSXVersionIndex == 0) // DSX v1 (FREE)
            {
                Process.Start(appSettings.DSXExecutablePath);
            }
            else // DSX v2/v3 (STEAM)
            {
                Process.Start("explorer", "steam://rungameid/1812620");
            }
            UpdateStatus("DSX started with game.");
        }

        private void EnsureDSXIsNotRunning()
        {
            string[] processNamesToKill = { "DSX", "DualSenseX" };

            foreach (string processName in processNamesToKill)
            {
                try
                {
                    var dsxProcesses = Process.GetProcesses().Where(p => p.ProcessName.Contains(processName) && !p.ProcessName.Contains("DSXGame"));

                    foreach (var dsxProcess in dsxProcesses)
                    {
                        try
                        {
                            dsxProcess.Kill();
                            dsxProcess.WaitForExit();
                            UpdateStatus($"{dsxProcess.ProcessName} (PID: {dsxProcess.Id}) stopped.");
                        }
                        catch (Exception ex)
                        {
                            UpdateStatus($"Error stopping {dsxProcess.ProcessName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Error getting processes for {processName}: {ex.Message}");
                }
            }
        }

        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        private void cbCheckInterval_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbCheckInterval.SelectedItem != null)
            {
                if (int.TryParse(cbCheckInterval.SelectedItem.ToString(), out int newInterval))
                {
                    checkInterval = newInterval * 1000; // Convert to milliseconds
                                                        // Stop the existing timer
                    processCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    // Create a new timer with the updated interval
                    processCheckTimer = new Timer(CheckRunningGames, null, 0, checkInterval);
                    SaveSettings();
                    UpdateStatus($"Check interval set to {newInterval} seconds.");
                }
                else
                {
                    // Handle the case where parsing fails, e.g., display an error message.
                    UpdateStatus("Invalid check interval selected.");
                }
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            processCheckTimer.Change(0, checkInterval);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            processCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            EnsureDSXIsNotRunning();
        }

    }

    public class GameInfo
    {
        public string GameName { get; set; }
        public string GamePath { get; set; }
    }

    public class Settings
    {
        public Settings()
        {
            DSXVersionIndex = 0; // Default to DSX v1 (FREE)
            CheckInterval = 1;  // Default to 1 second

        }
        public string LastUsedDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        public List<GameInfo> GamePaths { get; set; } = new List<GameInfo>();
        public string DSXExecutablePath { get; set; }
        public int DSXVersionIndex { get; set; }
        public int CheckInterval { get; set; } = 1; // Default to 5 seconds
    }
}
