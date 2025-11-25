using System;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Represents a single message in the Candy-Bot chat
    /// </summary>
    public class CandyBotChatMessage
    {
        public string Sender { get; set; } = "";
        public string SenderColor { get; set; } = "#FFFFFF";
        public string Message { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public string Background { get; set; } = "#1A1A1A";
        public string BorderBrush { get; set; } = "#00FF00";
        public bool IsBot { get; set; }
        
        public static CandyBotChatMessage CreateUserMessage(string message)
        {
            return new CandyBotChatMessage
            {
                Sender = "You",
                SenderColor = "#00FF00",
                Message = message,
                Timestamp = DateTime.Now.ToString("HH:mm"),
                Background = "#001100",
                BorderBrush = "#00FF00",
                IsBot = false
            };
        }
        
        public static CandyBotChatMessage CreateBotMessage(string message)
        {
            return new CandyBotChatMessage
            {
                Sender = "Candy-Bot",
                SenderColor = "#FF69B4",
                Message = message,
                Timestamp = DateTime.Now.ToString("HH:mm"),
                Background = "#1A0A10",
                BorderBrush = "#FF69B4",
                IsBot = true
            };
        }
    }
}
