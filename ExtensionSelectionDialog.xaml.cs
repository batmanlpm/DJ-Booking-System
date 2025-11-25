using System;
using System.Windows;
using System.Windows.Input;

namespace DJBookingSystem
{
    public partial class ExtensionSelectionDialog : Window
    {
        public string Extensions { get; private set; } = string.Empty;

        public ExtensionSelectionDialog(string currentExtensions)
        {
            InitializeComponent();
            ExtensionTextBox.Text = string.IsNullOrEmpty(currentExtensions) ? ".mp3, .wav, .flac" : currentExtensions;
        }

        private void QuickSearch_Music(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".mp3, .wav, .flac, .m4a, .aac, .ogg, .wma";
        }

        private void QuickSearch_Documents(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".pdf, .docx, .doc, .txt, .xlsx, .xls, .pptx, .ppt";
        }

        private void QuickSearch_Images(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".jpg, .jpeg, .png, .gif, .bmp, .svg, .webp";
        }

        private void QuickSearch_Videos(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".mp4, .avi, .mkv, .mov, .wmv, .flv, .webm";
        }

        private void QuickSearch_Archives(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".zip, .rar, .7z, .tar, .gz, .bz2";
        }

        private void QuickSearch_Code(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".cs, .js, .py, .java, .cpp, .h, .html, .css, .sql";
        }

        private void QuickSearch_World(object sender, RoutedEventArgs e)
        {
            ExtensionTextBox.Text = ".world";
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            string input = ExtensionTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Please enter at least one file extension!", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Extensions = input;
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
