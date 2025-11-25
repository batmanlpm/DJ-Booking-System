using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class CandyBotWindow : Window
    {
        private readonly CandyBotService? _candyBot;
        private readonly ObservableCollection<CandyBotChatMessage> _chatHistory;
        private readonly User? _currentUser;
        private readonly CosmosDbService? _cosmosDb;

        public CandyBotWindow(User? currentUser, CandyBotService? candyBot, CosmosDbService? cosmosDb = null)
        {
            InitializeComponent();
            
            _currentUser = currentUser;
            _candyBot = candyBot;
            _cosmosDb = cosmosDb;
            _chatHistory = new ObservableCollection<CandyBotChatMessage>();
            
            ChatHistoryItems.ItemsSource = _chatHistory;

            // Welcome message with proper icons
            AddBotMessage($"Hi {currentUser?.FullName ?? "there"}! I'm Candy-Bot, your AI assistant. How can I help you today?");
            
            // Focus on input
            UserInputTextBox.Focus();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to maximize/restore
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                // Single click to drag
                DragMove();
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendUserMessageAsync();
        }

        private async void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendUserMessageAsync();
            }
        }

        private async Task SendUserMessageAsync()
        {
            string userMessage = UserInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;

            // Add user message to chat
            AddUserMessage(userMessage);
            UserInputTextBox.Clear();

            // Show typing indicator
            AddBotMessage("Thinking...");

            try
            {
                // Get booking count if cosmos service available
                int bookingCount = 0;
                if (_cosmosDb != null)
                {
                    try
                    {
                        var allBookings = await _cosmosDb.GetAllBookingsAsync();
                        bookingCount = allBookings?.Count(b => b.DJName == _currentUser?.Username) ?? 0;
                    }
                    catch
                    {
                        // Ignore if can't get bookings
                    }
                }

                // Get AI response with context
                string response = await _candyBot?.GetContextualResponseAsync(
                    userMessage,
                    _currentUser,
                    bookingCount,
                    "CandyBot Chat"
                ) ?? "I'm sorry, I couldn't process that request.";

                // Remove typing indicator
                _chatHistory.RemoveAt(_chatHistory.Count - 1);

                // Add bot response
                AddBotMessage(response);
            }
            catch (Exception ex)
            {
                _chatHistory.RemoveAt(_chatHistory.Count - 1);
                AddBotMessage($"Oops! I encountered an error: {ex.Message}\n\nPlease try again in a moment!");
            }

            // Scroll to bottom
            ChatScrollViewer.ScrollToEnd();
            
            // Focus back on input
            UserInputTextBox.Focus();
        }

        private void AddUserMessage(string message)
        {
            _chatHistory.Add(CandyBotChatMessage.CreateUserMessage(message));
        }

        private void AddBotMessage(string message)
        {
            _chatHistory.Add(CandyBotChatMessage.CreateBotMessage(message));
        }

        // Quick Actions - Enhanced with Navigation
        private void QuickAction_BookSlot(object sender, RoutedEventArgs e)
        {
            // Show helpful instructions first
            var result = MessageBox.Show(
                "HOW TO BOOK A DJ SLOT\n\n" +
                "1. Click the 'Create Booking' or '+' button\n" +
                "2. Select your DJ name from dropdown\n" +
                "3. Choose the date and time\n" +
                "4. Select the venue\n" +
                "5. Add any notes (optional)\n" +
                "6. Click 'Save Booking'\n\n" +
                "Your booking will appear in the calendar!\n\n" +
                "Want to go to Bookings page now?",
                "Book a DJ Slot - Instructions",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            
            if (result == MessageBoxResult.Yes)
            {
                // Close chat window
                this.Close();
                
                // Get main window and navigate
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow?.ShowBookingsView();
            }
        }

        private void QuickAction_FindVenues(object sender, RoutedEventArgs e)
        {
            // Show helpful instructions first
            var result = MessageBox.Show(
                "BROWSE AVAILABLE VENUES\n\n" +
                "You can:\n" +
                "• View all registered venues\n" +
                "• See venue details and capacity\n" +
                "• Check contact information\n" +
                "• Create new venue (if admin)\n\n" +
                "Tip: Click on a venue to see more details!\n\n" +
                "Want to go to Venues page now?",
                "Find Venues - Instructions",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            
            if (result == MessageBoxResult.Yes)
            {
                // Close chat window
                this.Close();
                
                // Get main window and navigate
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow?.ShowVenuesView();
            }
        }

        private void QuickAction_GetTips(object sender, RoutedEventArgs e)
        {
            // Show DJ tips immediately
            MessageBox.Show(
                "DJ TIPS FROM CANDY-BOT\n\n" +
                "BEFORE THE GIG:\n" +
                "• Prepare 2-3 sets for different moods\n" +
                "• Test all equipment 30 min early\n" +
                "• Have backup USB/laptop ready\n" +
                "• Bring your own cables & adapters\n\n" +
                "DURING THE SET:\n" +
                "• Read the crowd energy\n" +
                "• Mix genres smoothly\n" +
                "• Take requests but stay true to vibe\n" +
                "• Keep energy building gradually\n\n" +
                "PRO TIPS:\n" +
                "• Hydrate frequently\n" +
                "• Network during breaks\n" +
                "• Record your sets\n" +
                "• Always end on a high note!\n\n" +
                "You've got this!",
                "DJ Tips from Candy-Bot",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
                
            // Close chat after showing tips
            this.Close();
        }

        private void QuickAction_GetHelp(object sender, RoutedEventArgs e)
        {
            // Show navigation help first
            var result = MessageBox.Show(
                "HELP & NAVIGATION\n\n" +
                "MAIN SECTIONS:\n" +
                "• Bookings - Manage DJ schedules\n" +
                "• Venues - Browse locations\n" +
                "• Radio - Stream controls\n" +
                "• Chat - Communication hub\n" +
                "• Users - User management\n" +
                "• Settings - Customize app\n\n" +
                "CANDY-BOT FEATURES:\n" +
                "• Right-click me for context menu\n" +
                "• Click me to open chat\n" +
                "• Ask me anything!\n\n" +
                "Want to view full documentation?",
                "Help & Navigation Guide",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            
            if (result == MessageBoxResult.Yes)
            {
                // Close chat window
                this.Close();
                
                // Get main window and navigate
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow?.ShowHelpView();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
