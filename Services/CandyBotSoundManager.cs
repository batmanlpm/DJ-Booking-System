using System;
using System.IO;
using System.Media;
using System.Windows.Media;
using DJBookingSystem.Models; // For CandyBotPersonalityMode
using System.Threading.Tasks; // For async Task

// Suppress CS4014 for this file: Audio playback is intentionally fire-and-forget
// Sound effects and voice lines don't need to be awaited - they're background audio
#pragma warning disable CS4014

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Manages Candy-Bot voice lines and sound effects
    /// Uses pre-recorded voice files (001-100) from Voices folder
    /// </summary>
    public class CandyBotSoundManager : IDisposable
    {
        private MediaPlayer _mediaPlayer;
        private bool _soundsEnabled = true;
        private bool _voiceModeEnabled = false;
        private Random _random = new Random();
        
        // Current personality mode affects voice line selection
        private CandyBotPersonalityMode _currentPersonality = CandyBotPersonalityMode.Normal;

        public CandyBotSoundManager()
        {
            _mediaPlayer = new MediaPlayer();
        }

        /// <summary>
        /// Enable or disable sound effects
        /// </summary>
        public void SetSoundsEnabled(bool enabled)
        {
            _soundsEnabled = enabled;
        }

        /// <summary>
        /// Enable or disable voice mode (plays pre-recorded voice lines)
        /// </summary>
        public void SetVoiceMode(bool enabled)
        {
            _voiceModeEnabled = enabled;
            System.Diagnostics.Debug.WriteLine($"Voice mode: {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Set current personality mode
        /// </summary>
        public void SetPersonality(CandyBotPersonalityMode mode)
        {
            _currentPersonality = mode;
        }

        // ==================== VOICE LINE PLAYBACK ====================

        /// <summary>
        /// Play a specific voice line by number (001-100)
        /// </summary>
        public Task PlayVoiceLine(string lineNumber)
        {
            if (!_voiceModeEnabled || !_soundsEnabled) return Task.CompletedTask;

            try
            {
                var voiceLine = CandyBotVoiceMapper.GetLine(lineNumber);
                if (voiceLine != null && File.Exists(voiceLine.FilePath))
                {
                    PlayVoiceFile(voiceLine.FilePath);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Voice line {lineNumber} not found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing voice line: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Play a random voice line from a category
        /// </summary>
        public Task PlayRandomVoiceFromCategory(string category)
        {
            if (!_voiceModeEnabled || !_soundsEnabled) return Task.CompletedTask;

            try
            {
                var voiceLine = CandyBotVoiceMapper.GetRandomLineFromCategory(category);
                if (voiceLine != null && File.Exists(voiceLine.FilePath))
                {
                    PlayVoiceFile(voiceLine.FilePath);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No voice lines found for category: {category}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing random voice: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Play voice file from path
        /// </summary>
        private void PlayVoiceFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Voice file not found: {filePath}");
                    return;
                }

                _mediaPlayer.Open(new Uri(filePath, UriKind.Relative));
                _mediaPlayer.Play();
                System.Diagnostics.Debug.WriteLine($"Playing voice: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing voice file: {ex.Message}");
            }
        }

        // ==================== CONTEXT-SPECIFIC VOICE LINES ====================

        /// <summary>
        /// Play greeting based on personality mode
        /// </summary>
        public void PlayGreeting()
        {
            if (!_voiceModeEnabled) return;

            switch (_currentPersonality)
            {
                case CandyBotPersonalityMode.Shy:
                    PlayVoiceLine("081"); // Shy greeting
                    break;
                case CandyBotPersonalityMode.Professional:
                    PlayVoiceLine("094"); // Professional greeting
                    break;
                case CandyBotPersonalityMode.Raunchy:
                    PlayVoiceLine("097"); // Raunchy greeting
                    break;
                default:
                    // Random normal greeting (001-015)
                    PlayRandomVoiceFromCategory("Greeting");
                    break;
            }
        }

        /// <summary>
        /// Play help response
        /// </summary>
        public void PlayHelpResponse()
        {
            if (!_voiceModeEnabled) return;
            PlayRandomVoiceFromCategory("Help");
        }

        /// <summary>
        /// Play booking assistance voice
        /// </summary>
        public void PlayBookingResponse()
        {
            if (!_voiceModeEnabled) return;
            PlayRandomVoiceFromCategory("Booking");
        }

        /// <summary>
        /// Play positive feedback
        /// </summary>
        public void PlayPositiveFeedback()
        {
            if (!_voiceModeEnabled) return;

            if (_currentPersonality == CandyBotPersonalityMode.Funny)
            {
                PlayRandomVoiceFromCategory("Funny");
            }
            else
            {
                PlayRandomVoiceFromCategory("Positive");
            }
        }

        /// <summary>
        /// Play error/apology response
        /// </summary>
        public void PlayErrorResponse()
        {
            if (!_voiceModeEnabled) return;
            PlayRandomVoiceFromCategory("Error");
        }

        /// <summary>
        /// Play personality/fun response
        /// </summary>
        public void PlayPersonalityResponse()
        {
            if (!_voiceModeEnabled) return;

            switch (_currentPersonality)
            {
                case CandyBotPersonalityMode.Shy:
                    PlayRandomVoiceFromCategory("Shy");
                    break;
                case CandyBotPersonalityMode.Funny:
                    PlayRandomVoiceFromCategory("Funny");
                    break;
                case CandyBotPersonalityMode.ShitStirring:
                    PlayRandomVoiceFromCategory("ShitStirring");
                    break;
                case CandyBotPersonalityMode.Professional:
                    PlayRandomVoiceFromCategory("Professional");
                    break;
                case CandyBotPersonalityMode.Raunchy:
                    PlayRandomVoiceFromCategory("Raunchy");
                    break;
                default:
                    PlayRandomVoiceFromCategory("Personality");
                    break;
            }
        }

        // ==================== SYSTEM SOUNDS ====================

        /// <summary>
        /// Play system sound effect
        /// </summary>
        public void PlaySoundEffect(string effectName)
        {
            if (!_soundsEnabled) return;

            try
            {
                switch (effectName.ToLower())
                {
                    case "notification":
                        SystemSounds.Beep.Play();
                        break;
                    case "error":
                        SystemSounds.Hand.Play();
                        break;
                    case "success":
                        SystemSounds.Asterisk.Play();
                        break;
                    case "question":
                        SystemSounds.Question.Play();
                        break;
                    case "exclamation":
                        SystemSounds.Exclamation.Play();
                        break;
                    default:
                        SystemSounds.Beep.Play();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound effect: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop current voice playback
        /// </summary>
        public void StopSpeaking()
        {
            try
            {
                _mediaPlayer.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping playback: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _mediaPlayer?.Close();
        }

        // ==================== LEGACY METHODS (for backward compatibility) ====================

        /// <summary>
        /// Speak text (legacy method - redirects to voice line playback)
        /// </summary>
        public void Speak(string text)
        {
            if (!_voiceModeEnabled || string.IsNullOrWhiteSpace(text)) return;
            
            // Try to match text to appropriate category
            string lowerText = text.ToLower();
            
            if (lowerText.Contains("hello") || lowerText.Contains("hi") || lowerText.Contains("welcome"))
            {
                PlayGreeting();
            }
            else if (lowerText.Contains("help"))
            {
                PlayHelpResponse();
            }
            else if (lowerText.Contains("book") || lowerText.Contains("venue"))
            {
                PlayBookingResponse();
            }
            else if (lowerText.Contains("success") || lowerText.Contains("done") || lowerText.Contains("complete"))
            {
                PlayPositiveFeedback();
            }
            else if (lowerText.Contains("error") || lowerText.Contains("sorry") || lowerText.Contains("oops"))
            {
                PlayErrorResponse();
            }
            else
            {
                PlayPersonalityResponse();
            }
        }

        /// <summary>
        /// Play notification sound (legacy method)
        /// </summary>
        public void PlayNotification()
        {
            PlaySoundEffect("notification");
        }

        /// <summary>
        /// Play success sound (legacy method)
        /// </summary>
        public void PlaySuccess()
        {
            if (_voiceModeEnabled)
            {
                PlayPositiveFeedback();
            }
            else
            {
                PlaySoundEffect("success");
            }
        }

        /// <summary>
        /// Play error sound (legacy method)
        /// </summary>
        public void PlayError()
        {
            if (_voiceModeEnabled)
            {
                PlayErrorResponse();
            }
            else
            {
                PlaySoundEffect("error");
            }
        }
    }
}
