using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class CustomerChatWindow : Window
    {
        private readonly ExtensiveChatBot _chatBot;

        public CustomerChatWindow()
        {
            InitializeComponent();
            _chatBot = new ExtensiveChatBot();
            
            LoadCategories();
            ShowWelcomeMessage();
        }

        private void LoadCategories()
        {
            CategoryListBox.Items.Clear();
            foreach (var category in _chatBot.GetAllCategories())
            {
                CategoryListBox.Items.Add(category);
            }
        }

        private void ShowWelcomeMessage()
        {
            AddBotMessage("👋 Hi! I'm your customer support assistant. I can help with:\n\n" +
                         "• Account & Login\n" +
                         "• Billing & Subscriptions\n" +
                         "• Technical Issues\n" +
                         "• Content & Features\n" +
                         "• Privacy & Security\n\n" +
                         "What can I help you with today?");
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync()
        {
            var message = UserInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            AddUserMessage(message);
            UserInputTextBox.Clear();

            var response = await _chatBot.GetResponseAsync(message);
            
            AddBotMessage(response.Response);
            
            if (response.SuggestedQuestions.Any())
            {
                ShowSuggestedQuestions(response.SuggestedQuestions);
            }
            else
            {
                SuggestionsPanel.Visibility = Visibility.Collapsed;
            }

            ChatScrollViewer.ScrollToEnd();
        }

        private void AddUserMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 40, 0)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(50, 5, 5, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 450
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            ChatMessagesPanel.Children.Add(border);
        }

        private void AddBotMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 17, 0)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(5, 5, 50, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 450
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            ChatMessagesPanel.Children.Add(border);
        }

        private void ShowSuggestedQuestions(System.Collections.Generic.List<string> suggestions)
        {
            SuggestedQuestionsList.ItemsSource = suggestions;
            SuggestionsPanel.Visibility = Visibility.Visible;
        }

        private void SuggestedQuestion_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Content is string question)
            {
                UserInputTextBox.Text = question;
                _ = SendMessageAsync();
            }
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is string category)
            {
                var questions = _chatBot.GetQuestionsByCategory();
                if (questions.ContainsKey(category))
                {
                    QuickQuestionsListBox.ItemsSource = questions[category];
                }
            }
        }

        private void QuickQuestion_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (QuickQuestionsListBox.SelectedItem is string question)
            {
                UserInputTextBox.Text = question;
                _ = SendMessageAsync();
            }
        }
    }
}
