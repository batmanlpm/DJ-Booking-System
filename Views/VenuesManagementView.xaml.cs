using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// Standalone Venues Management View
    /// Simplified view for managing venues only
    /// </summary>
    public partial class VenuesManagementView : UserControl
    {
        private List<Venue> _venues = new List<Venue>();
        private CosmosDbService? _cosmosService;
        private User? _currentUser;

        public VenuesManagementView()
        {
            InitializeComponent();
            _venues = new List<Venue>();
        }

        public async Task Initialize(CosmosDbService cosmosService, User currentUser)
        {
            _cosmosService = cosmosService ?? throw new ArgumentNullException(nameof(cosmosService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            
            // Show loading overlay
            if (LoadingOverlay != null)
                LoadingOverlay.Visibility = Visibility.Visible;
            
            try
            {
                if (LoadingMessage != null)
                    LoadingMessage.Text = "Loading venues...";
                
                await LoadVenuesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize venues: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadVenuesAsync()
        {
            try
            {
                if (_cosmosService == null) return;

                // Use ConfigureAwait(false) to not block UI thread
                var venues = await _cosmosService.GetAllVenuesAsync().ConfigureAwait(false);
                
                // Return to UI thread for UI updates
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _venues = venues.Where(v => v.IsActive).OrderBy(v => v.Name).ToList();
                    
                    if (VenuesDataGrid != null)
                        VenuesDataGrid.ItemsSource = _venues;
                });
            }
            catch (Exception ex)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Failed to load venues: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async void NewVenueButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosService == null || _currentUser == null) return;

            var createWindow = new CreateVenueWindow(_cosmosService, _currentUser);
            if (createWindow.ShowDialog() == true)
            {
                await LoadVenuesAsync();
            }
        }

        private void EditVenueButton_Click(object sender, RoutedEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is Venue selectedVenue)
            {
                MessageBox.Show("Edit venue feature coming soon!", "Information", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a venue to edit.", "Warning", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteVenueButton_Click(object sender, RoutedEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is not Venue selectedVenue || _cosmosService == null)
            {
                MessageBox.Show("Please select a venue to delete.", "Warning", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Delete venue '{selectedVenue.Name}'? This will also delete all bookings for this venue!", 
                "Confirm Delete", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                if (!string.IsNullOrEmpty(selectedVenue.Id))
                {
                    await _cosmosService.DeleteVenueAsync(selectedVenue.Id);
                    MessageBox.Show("Venue deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVenuesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete venue: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshVenuesButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadVenuesAsync();
        }
    }
}
