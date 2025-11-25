using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DJBookingSystem
{
    public partial class SearchConfirmationDialog : Window
    {
        public SearchConfirmationDialog(List<string> selectedDrives, string extensions, FileOperation? action, string destination)
        {
            InitializeComponent();

            // Display selected drives
            DrivesText.Text = selectedDrives.Count > 0 
                ? string.Join(", ", selectedDrives)
                : "No drives selected";

            // Display extensions
            ExtensionsText.Text = extensions ?? "No extensions specified";

            // Display action
            if (action.HasValue)
            {
                ActionText.Text = action.Value switch
                {
                    FileOperation.Copy => "Copy files to destination after search",
                    FileOperation.Move => "Move files to destination after search",
                    FileOperation.Delete => "Delete files after search (BE CAREFUL!)",
                    _ => "None (Search only)"
                };

                if (action.Value != FileOperation.Delete && !string.IsNullOrEmpty(destination))
                {
                    DestinationInfo.Visibility = Visibility.Visible;
                    DestinationText.Text = destination;
                }
            }
            else
            {
                ActionText.Text = "None (Search only - action can be chosen after results)";
            }
        }

        private void StartSearch_Click(object sender, RoutedEventArgs e)
        {
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
