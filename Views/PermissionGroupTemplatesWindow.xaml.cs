using System;
using System.Windows;
using System.Windows.Input;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// Permission Group Templates Window
    /// Shows predefined permission group templates for quick assignment
    /// </summary>
    public partial class PermissionGroupTemplatesWindow : Window
    {
        public PermissionGroupTemplatesWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Allow window dragging
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Close window
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
