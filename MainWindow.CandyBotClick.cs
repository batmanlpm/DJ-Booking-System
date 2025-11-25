using System;
using System.Windows;

namespace DJBookingSystem
{
    public partial class MainWindow
    {
        /// <summary>
        /// CandyBot Avatar Click Handler - Opens CandyBot chat window
        /// </summary>
        private void CandyBot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ?? Candy-Bot connected to live Railway API with enhanced DJ booking knowledge
                var candyBotService = new Services.CandyBotService(
                    apiKey: "not-needed-for-claude",
                    apiEndpoint: "https://candybot.livepartymusic.fm/api/conversation/chat"
                );

                // Open Candy-Bot window
                var candyBotWindow = new CandyBotWindow(_currentUser, candyBotService, _cosmosDbService);
                candyBotWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open Candy-Bot: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
