using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// Standalone maximizable window for User Management
    /// Hosts the UsersView control in a resizable, maximizable window
    /// </summary>
    public partial class UsersManagementWindow : Window
    {
        public UsersManagementWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the Users panel with database service and current user
        /// </summary>
        public async Task Initialize(CosmosDbService cosmosService, User currentUser)
        {
            await UsersViewControl.Initialize(cosmosService, currentUser);
        }

        /// <summary>
        /// Allow dragging window by title bar
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Minimize window
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Toggle maximize/restore
        /// </summary>
        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "?";
                MaximizeButton.ToolTip = "Maximize";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "?";
                MaximizeButton.ToolTip = "Restore";
            }
        }

        /// <summary>
        /// Close window
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
