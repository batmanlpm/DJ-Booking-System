using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class DriveSelectionDialog : Window
    {
        public List<string> SelectedDrives { get; private set; }
        private readonly MultiDriveFileSearcher _searcher;

        public DriveSelectionDialog(MultiDriveFileSearcher searcher, List<string> currentSelection)
        {
            InitializeComponent();
            _searcher = searcher;
            SelectedDrives = new List<string>(currentSelection);
            LoadDrives();
        }

        private void LoadDrives()
        {
            try
            {
                var drives = _searcher.GetAvailableDrives();

                foreach (var drive in drives)
                {
                    var checkBox = new CheckBox
                    {
                        Content = $"{drive.DriveLetter} ({drive.VolumeLabel}) - {drive.DriveType}",
                        Tag = drive.DriveLetter,
                        Margin = new Thickness(5),
                        Foreground = System.Windows.Media.Brushes.White,
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        FontSize = 12,
                        IsChecked = SelectedDrives.Contains(drive.DriveLetter)
                    };

                    DriveStackPanel.Children.Add(checkBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drives: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectAllDrives_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in DriveStackPanel.Children)
            {
                if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = true;
                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedDrives.Clear();
            foreach (var child in DriveStackPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox.IsChecked == true && checkBox.Tag is string driveLetter)
                {
                    SelectedDrives.Add(driveLetter);
                }
            }

            if (SelectedDrives.Count == 0)
            {
                MessageBox.Show("Please select at least one drive!", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
    }
}
