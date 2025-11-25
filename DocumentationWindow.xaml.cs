using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DJBookingSystem
{
    public partial class DocumentationWindow : Window
    {
        private string _currentDocPath = string.Empty;
        private readonly string _docsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CandyBot_Training_Guides");

        public DocumentationWindow()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"[DocCenter] Docs folder: {_docsFolder}");
        }

        private void Doc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string docName)
            {
                LoadDocument(docName);
            }
        }

        private void LoadDocument(string docName)
        {
            try
            {
                // Map docName to filename
                string filename = $"{docName}.md";
                string filePath = Path.Combine(_docsFolder, filename);

                if (!File.Exists(filePath))
                {
                    DocTitle.Text = $"? Document Not Found";
                    DocContent.Text = $"Could not find: {filename}\n\nPath: {filePath}\n\nMake sure the documentation files are in the CandyBot_Training_Guides folder.";
                    ActionButtons.Visibility = Visibility.Collapsed;
                    _currentDocPath = string.Empty;
                    return;
                }

                // Load content
                string content = File.ReadAllText(filePath);
                
                // Format title
                DocTitle.Text = FormatTitle(docName);
                DocContent.Text = content;
                ActionButtons.Visibility = Visibility.Visible;
                _currentDocPath = filePath;

                // Scroll to top
                ContentScroll.ScrollToTop();

                System.Diagnostics.Debug.WriteLine($"[DocCenter] Loaded: {filename}");
            }
            catch (Exception ex)
            {
                DocTitle.Text = "? Error Loading Document";
                DocContent.Text = $"Error: {ex.Message}\n\nStack trace:\n{ex.StackTrace}";
                ActionButtons.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[DocCenter] Error: {ex.Message}");
            }
        }

        private string FormatTitle(string docName)
        {
            // Convert doc name to readable title
            return docName
                .Replace("CANDYBOT_", "")
                .Replace("_", " ")
                .Trim();
        }

        private void OpenInEditor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentDocPath) || !File.Exists(_currentDocPath))
            {
                MessageBox.Show("No document is currently loaded.", "Cannot Open", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Open in default text editor
                Process.Start(new ProcessStartInfo
                {
                    FileName = _currentDocPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open editor:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentDocPath))
            {
                MessageBox.Show("No document is currently loaded.", "Cannot Copy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(_currentDocPath);
                MessageBox.Show($"Path copied to clipboard:\n\n{_currentDocPath}", "Copied!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy path:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
