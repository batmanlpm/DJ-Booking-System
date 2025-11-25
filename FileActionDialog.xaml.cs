using System;
using System.Windows;
using System.Windows.Input;

namespace DJBookingSystem
{
    public enum FileOperation
    {
        Copy,
        Move,
        Delete
    }

    public partial class FileActionDialog : Window
    {
        public FileOperation SelectedOperation { get; private set; }
        public string? DestinationPath { get; private set; }

        public FileActionDialog(int selectedFileCount, string defaultDestination)
        {
            InitializeComponent();
            
            try
            {
                SelectionInfoText.Text = $"{selectedFileCount} file(s) selected";
                SelectedOperation = FileOperation.Copy;
                DestinationPath = defaultDestination;
            }
            catch (Exception ex)
            {
                SelectionInfoText.Text = "Files selected";
                SelectedOperation = FileOperation.Copy;
                DestinationPath = defaultDestination;
                System.Diagnostics.Debug.WriteLine($"FileActionDialog init error: {ex.Message}");
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (CopyRadio.IsChecked == true)
            {
                SelectedOperation = FileOperation.Copy;
            }
            else if (MoveRadio.IsChecked == true)
            {
                SelectedOperation = FileOperation.Move;
            }
            else
            {
                SelectedOperation = FileOperation.Delete;
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
