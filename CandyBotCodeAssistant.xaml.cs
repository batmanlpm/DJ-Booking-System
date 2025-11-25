using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class CandyBotCodeAssistant : Window
    {
        private readonly User _currentUser;
        private readonly dynamic _candyBot;
        private readonly CosmosDbService? _cosmosDb;
        private readonly CandyBotTextToSpeech _tts;
        private readonly ObservableCollection<CandyChatMessage> _chatMessages;
        private bool _voiceEnabled = false;
        private string _currentLanguage = "csharp";

        public CandyBotCodeAssistant(User currentUser, dynamic candyBot, CosmosDbService? cosmosDb = null)
        {
            InitializeComponent();

            _currentUser = currentUser;
            _candyBot = candyBot;
            _cosmosDb = cosmosDb;
            _tts = new CandyBotTextToSpeech();
            _chatMessages = new ObservableCollection<CandyChatMessage>();

            ChatHistory.ItemsSource = _chatMessages;

            // Welcome message
            AddCandyMessage("?? Hi! I'm Candy-Bot, your coding expert!\n\nI can help you:\n• Generate code in any language\n• Analyze and debug code\n• Explain programming concepts\n• Create full applications\n\nWhat would you like to code today?");
        }

        #region Title Bar Events

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _tts?.Dispose();
            Close();
        }

        #endregion

        #region Voice Toggle

        private void ToggleVoice_Click(object sender, RoutedEventArgs e)
        {
            _voiceEnabled = !_voiceEnabled;
            VoiceToggleButton.Content = _voiceEnabled ? "??" : "??";
            _tts.SetEnabled(_voiceEnabled);

            if (_voiceEnabled)
            {
                _ = _tts.SpeakAsync("Voice mode enabled!");
            }
        }

        #endregion

        #region Language Selection

        private void Language_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem item)
            {
                _currentLanguage = item.Tag?.ToString() ?? "csharp";
                LanguageText.Text = $"Language: {item.Content}";
                StatusText.Text = $"?? Ready to code in {item.Content}!";
            }
        }

        #endregion

        #region Code Generation

        private async void GenerateCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionInput.Text))
            {
                MessageBox.Show("Please describe what you want to generate!", "Input Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string prompt = QuestionInput.Text;
            string template = (TemplateComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "class";

            StatusText.Text = "?? Generating code...";

            try
            {
                // Create AI prompt for code generation
                string aiPrompt = $"Generate {_currentLanguage} code for a {template} that: {prompt}\n\nProvide clean, well-commented code with best practices.";

                // Get response from Candy-Bot AI
                string response = await _candyBot.GetContextualResponseAsync(
                    aiPrompt,
                    _currentUser,
                    0,
                    "CodeAssistant"
                );

                // Extract code from response (remove markdown if present)
                string code = ExtractCodeFromResponse(response);

                // Display in code editor
                CodeEditor.Text = code;

                // Update line count
                UpdateLineCount();

                // Add to chat
                AddUserMessage($"Generate: {prompt}");
                AddCandyMessage($"Here's your {_currentLanguage} {template}! ??");

                // Voice feedback
                if (_voiceEnabled)
                {
                    await _tts.SpeakAsync("Code generated successfully!");
                }

                StatusText.Text = $"?? Generated {_currentLanguage} {template}!";
            }
            catch (Exception ex)
            {
                StatusText.Text = "? Generation failed!";
                AddCandyMessage($"?? Error: {ex.Message}");
                
                if (_voiceEnabled)
                {
                    await _tts.SayErrorAsync();
                }
            }
        }

        #endregion

        #region Code Analysis

        private async void AnalyzeCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeEditor.Text))
            {
                MessageBox.Show("Please enter some code to analyze!", "No Code", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StatusText.Text = "?? Analyzing code...";

            try
            {
                string code = CodeEditor.Text;
                string aiPrompt = $"Analyze this {_currentLanguage} code and provide:\n1. Code review feedback\n2. Potential bugs\n3. Performance improvements\n4. Best practice suggestions\n\nCode:\n{code}";

                string analysis = await _candyBot.GetContextualResponseAsync(
                    aiPrompt,
                    _currentUser,
                    0,
                    "CodeAnalysis"
                );

                AddUserMessage("Analyze my code");
                AddCandyMessage($"?? Analysis Results:\n\n{analysis}");

                if (_voiceEnabled)
                {
                    await _tts.SpeakAsync("Analysis complete!");
                }

                StatusText.Text = "?? Analysis complete!";
            }
            catch (Exception ex)
            {
                StatusText.Text = "? Analysis failed!";
                AddCandyMessage($"?? Error: {ex.Message}");
            }
        }

        #endregion

        #region File Operations

        private void SaveCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeEditor.Text))
            {
                MessageBox.Show("No code to save!", "Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = GetFileFilter(),
                FileName = $"CandyBot_Generated.{GetFileExtension()}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllText(dialog.FileName, CodeEditor.Text);
                    StatusText.Text = $"?? Saved to {System.IO.Path.GetFileName(dialog.FileName)}";
                    AddCandyMessage($"? Code saved successfully!");

                    if (_voiceEnabled)
                    {
                        _ = _tts.SaySuccessAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeEditor.Text))
            {
                MessageBox.Show("No code to copy!", "Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Clipboard.SetText(CodeEditor.Text);
                StatusText.Text = "?? Code copied to clipboard!";
                AddCandyMessage("?? Code copied! Paste it anywhere!");

                if (_voiceEnabled)
                {
                    _ = _tts.SpeakAsync("Copied!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying: {ex.Message}", "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Chat with Candy

        private async void AskCandy_Click(object sender, RoutedEventArgs e)
        {
            await AskCandyAsync();
        }

        private async void QuestionInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await AskCandyAsync();
            }
        }

        private async Task AskCandyAsync()
        {
            string question = QuestionInput.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;

            AddUserMessage(question);
            QuestionInput.Clear();

            StatusText.Text = "?? Thinking...";

            try
            {
                string response = await _candyBot.GetContextualResponseAsync(
                    question,
                    _currentUser,
                    0,
                    "CodeAssistant"
                );

                AddCandyMessage(response);

                if (_voiceEnabled)
                {
                    await _tts.SpeakAsync(response);
                }

                StatusText.Text = "?? Ready for your next question!";
            }
            catch (Exception ex)
            {
                AddCandyMessage($"?? Error: {ex.Message}");
                StatusText.Text = "? Error occurred!";
            }

            ChatScrollViewer?.ScrollToEnd();
        }

        #endregion

        #region Helper Methods

        private void AddUserMessage(string message)
        {
            _chatMessages.Add(new CandyChatMessage
            {
                Sender = _currentUser.FullName,
                Message = message,
                SenderColor = Brushes.Cyan
            });
            ChatScrollViewer?.ScrollToEnd();
        }

        private void AddCandyMessage(string message)
        {
            _chatMessages.Add(new CandyChatMessage
            {
                Sender = "Candy-Bot",
                Message = message,
                SenderColor = Brushes.HotPink
            });
            ChatScrollViewer?.ScrollToEnd();
        }

        private string ExtractCodeFromResponse(string response)
        {
            // Remove markdown code blocks if present
            if (response.Contains("```"))
            {
                int startIndex = response.IndexOf("```") + 3;
                // Skip language identifier line
                if (response.Length > startIndex)
                {
                    startIndex = response.IndexOf('\n', startIndex) + 1;
                }
                int endIndex = response.LastIndexOf("```");
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    return response.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            
            return response;
        }

        private void UpdateLineCount()
        {
            int lineCount = CodeEditor.Text.Split('\n').Length;
            LineCountText.Text = $"Lines: {lineCount}";
        }

        private string GetFileFilter()
        {
            return _currentLanguage switch
            {
                "csharp" => "C# Files (*.cs)|*.cs|All Files (*.*)|*.*",
                "python" => "Python Files (*.py)|*.py|All Files (*.*)|*.*",
                "javascript" => "JavaScript Files (*.js)|*.js|All Files (*.*)|*.*",
                "typescript" => "TypeScript Files (*.ts)|*.ts|All Files (*.*)|*.*",
                "html" => "HTML Files (*.html)|*.html|All Files (*.*)|*.*",
                "css" => "CSS Files (*.css)|*.css|All Files (*.*)|*.*",
                "sql" => "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*",
                "java" => "Java Files (*.java)|*.java|All Files (*.*)|*.*",
                "php" => "PHP Files (*.php)|*.php|All Files (*.*)|*.*",
                "cpp" => "C++ Files (*.cpp)|*.cpp|All Files (*.*)|*.*",
                _ => "All Files (*.*)|*.*"
            };
        }

        private string GetFileExtension()
        {
            return _currentLanguage switch
            {
                "csharp" => "cs",
                "python" => "py",
                "javascript" => "js",
                "typescript" => "ts",
                "html" => "html",
                "css" => "css",
                "sql" => "sql",
                "java" => "java",
                "php" => "php",
                "cpp" => "cpp",
                _ => "txt"
            };
        }

        #endregion
    }

    public class CandyChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Brush SenderColor { get; set; } = Brushes.White;
    }
}
