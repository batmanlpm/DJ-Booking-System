using System;
using System.IO;
using System.Windows;

namespace DJBookingSystem.InteractiveGuide
{
    public class GuideSelector
    {
        private readonly string _basePath;

        public GuideSelector(string basePath)
        {
            _basePath = basePath;
        }

        public string GetGuideAudio(string userRole, bool isIntroVersion)
        {
            bool isAdmin = (userRole == "SysAdmin" || userRole == "Manager");
            string roleType = isAdmin ? "Admin" : "User";
            string version = isIntroVersion ? "Intro" : "Detailed";
            
            string fileName = $"CandyBot_Guide_{roleType}_{version}.mp3";
            string fullPath = Path.Combine(_basePath, "Assets", "Audio", fileName);

            if (!File.Exists(fullPath))
            {
                System.Diagnostics.Debug.WriteLine($"Guide audio not found: {fileName}");
                return string.Empty;
            }

            return fullPath;
        }

        public void ShowIntroGuide(string userRole)
        {
            try
            {
                string audioPath = GetGuideAudio(userRole, isIntroVersion: true);
                if (!string.IsNullOrEmpty(audioPath))
                {
                    var player = new GuidePlayerWindow(audioPath);
                    player.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guide audio: {ex.Message}", 
                                "Guide Error", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }

        public void ShowDetailedGuide(string userRole)
        {
            try
            {
                string audioPath = GetGuideAudio(userRole, isIntroVersion: false);
                if (!string.IsNullOrEmpty(audioPath))
                {
                    var player = new GuidePlayerWindow(audioPath);
                    player.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guide audio: {ex.Message}", 
                                "Guide Error", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }
    }
}
