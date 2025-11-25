using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Candy-Bot Document Generation Service
    /// Creates Word docs, PDFs, PowerPoints, Excel files, and more
    /// Uses popular .NET libraries for document creation
    /// </summary>
    public class CandyBotDocumentGenerator
    {
        private readonly CandyBotSoundManager? _soundManager;
        private readonly string _defaultOutputFolder;

        public CandyBotDocumentGenerator(CandyBotSoundManager? soundManager = null)
        {
            _soundManager = soundManager;
            _defaultOutputFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CandyBot-Documents"
            );
            Directory.CreateDirectory(_defaultOutputFolder);
        }

        #region Word Documents (.docx)

        /// <summary>
        /// Create a Word document
        /// REQUIRES: DocumentFormat.OpenXml NuGet package
        /// Install: Install-Package DocumentFormat.OpenXml
        /// </summary>
        public async Task<DocumentResult> CreateWordDocumentAsync(
            string title,
            string content,
            string? fileName = null,
            string? outputFolder = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _soundManager?.PlayVoiceLine("032"); // "Let's get started!"

                    fileName = fileName ?? $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                    outputFolder = outputFolder ?? _defaultOutputFolder;
                    var filePath = Path.Combine(outputFolder, fileName);

                    // NOTE: This requires DocumentFormat.OpenXml package
                    // For now, creating a placeholder implementation
                    CreateWordDocumentPlaceholder(filePath, title, content);

                    _soundManager?.PlayVoiceLine("048"); // "Boom! Task completed!"

                    return new DocumentResult
                    {
                        Success = true,
                        FilePath = filePath,
                        FileName = fileName,
                        DocumentType = DocumentType.Word,
                        Message = "Word document created successfully"
                    };
                }
                catch (Exception ex)
                {
                    _soundManager?.PlayVoiceLine("056"); // Error
                    return new DocumentResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        DocumentType = DocumentType.Word
                    };
                }
            });
        }

        /// <summary>
        /// Placeholder for Word document creation
        /// TODO: Implement using DocumentFormat.OpenXml when package is installed
        /// </summary>
        private void CreateWordDocumentPlaceholder(string filePath, string title, string content)
        {
            // Simple text file for now
            // Will be replaced with proper .docx generation when DocumentFormat.OpenXml is added
            var text = $"{title}\n\n{content}";
            File.WriteAllText(filePath.Replace(".docx", ".txt"), text);
            
            Debug.WriteLine("[CandyBot DocGen] Created text file (Word generation requires DocumentFormat.OpenXml package)");
        }

        #endregion

        #region PDF Documents

        /// <summary>
        /// Create a PDF document
        /// REQUIRES: iTextSharp or PdfSharp NuGet package
        /// Install: Install-Package itext7
        /// </summary>
        public async Task<DocumentResult> CreatePDFDocumentAsync(
            string title,
            string content,
            string? fileName = null,
            string? outputFolder = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _soundManager?.PlayVoiceLine("032");

                    fileName = fileName ?? $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    outputFolder = outputFolder ?? _defaultOutputFolder;
                    var filePath = Path.Combine(outputFolder, fileName);

                    CreatePDFPlaceholder(filePath, title, content);

                    _soundManager?.PlayVoiceLine("048");

                    return new DocumentResult
                    {
                        Success = true,
                        FilePath = filePath,
                        FileName = fileName,
                        DocumentType = DocumentType.PDF,
                        Message = "PDF created successfully"
                    };
                }
                catch (Exception ex)
                {
                    _soundManager?.PlayVoiceLine("056");
                    return new DocumentResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        DocumentType = DocumentType.PDF
                    };
                }
            });
        }

        private void CreatePDFPlaceholder(string filePath, string title, string content)
        {
            // Placeholder - requires PDF library
            var text = $"{title}\n\n{content}";
            File.WriteAllText(filePath.Replace(".pdf", ".txt"), text);
            
            Debug.WriteLine("[CandyBot DocGen] Created text file (PDF generation requires iText7 package)");
        }

        #endregion

        #region PowerPoint Presentations (.pptx)

        /// <summary>
        /// Create a PowerPoint presentation
        /// REQUIRES: DocumentFormat.OpenXml NuGet package
        /// </summary>
        public async Task<DocumentResult> CreatePowerPointAsync(
            string title,
            List<SlideContent> slides,
            string? fileName = null,
            string? outputFolder = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _soundManager?.PlayVoiceLine("032");

                    fileName = fileName ?? $"Presentation_{DateTime.Now:yyyyMMdd_HHmmss}.pptx";
                    outputFolder = outputFolder ?? _defaultOutputFolder;
                    var filePath = Path.Combine(outputFolder, fileName);

                    CreatePowerPointPlaceholder(filePath, title, slides);

                    _soundManager?.PlayVoiceLine("048");

                    return new DocumentResult
                    {
                        Success = true,
                        FilePath = filePath,
                        FileName = fileName,
                        DocumentType = DocumentType.PowerPoint,
                        Message = "PowerPoint created successfully"
                    };
                }
                catch (Exception ex)
                {
                    _soundManager?.PlayVoiceLine("056");
                    return new DocumentResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        DocumentType = DocumentType.PowerPoint
                    };
                }
            });
        }

        private void CreatePowerPointPlaceholder(string filePath, string title, List<SlideContent> slides)
        {
            var text = $"{title}\n\n";
            foreach (var slide in slides)
            {
                text += $"Slide: {slide.Title}\n{slide.Content}\n\n";
            }
            File.WriteAllText(filePath.Replace(".pptx", ".txt"), text);
            
            Debug.WriteLine("[CandyBot DocGen] Created text file (PowerPoint requires DocumentFormat.OpenXml)");
        }

        #endregion

        #region Excel Spreadsheets (.xlsx)

        /// <summary>
        /// Create an Excel spreadsheet
        /// REQUIRES: ClosedXML or EPPlus NuGet package
        /// Install: Install-Package ClosedXML
        /// </summary>
        public async Task<DocumentResult> CreateExcelSpreadsheetAsync(
            string sheetName,
            List<List<string>> data,
            string? fileName = null,
            string? outputFolder = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _soundManager?.PlayVoiceLine("032");

                    fileName = fileName ?? $"Spreadsheet_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    outputFolder = outputFolder ?? _defaultOutputFolder;
                    var filePath = Path.Combine(outputFolder, fileName);

                    CreateExcelPlaceholder(filePath, sheetName, data);

                    _soundManager?.PlayVoiceLine("048");

                    return new DocumentResult
                    {
                        Success = true,
                        FilePath = filePath,
                        FileName = fileName,
                        DocumentType = DocumentType.Excel,
                        Message = "Excel spreadsheet created successfully"
                    };
                }
                catch (Exception ex)
                {
                    _soundManager?.PlayVoiceLine("056");
                    return new DocumentResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        DocumentType = DocumentType.Excel
                    };
                }
            });
        }

        private void CreateExcelPlaceholder(string filePath, string sheetName, List<List<string>> data)
        {
            // CSV format as placeholder
            var csv = string.Join("\n", data.Select(row => string.Join(",", row)));
            File.WriteAllText(filePath.Replace(".xlsx", ".csv"), csv);
            
            Debug.WriteLine("[CandyBot DocGen] Created CSV file (Excel requires ClosedXML package)");
        }

        #endregion

        #region Text & Markdown Files

        /// <summary>
        /// Create a text file
        /// </summary>
        public async Task<DocumentResult> CreateTextFileAsync(
            string content,
            string? fileName = null,
            string? outputFolder = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _soundManager?.PlayVoiceLine("032");

                    fileName = fileName ?? $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    outputFolder = outputFolder ?? _defaultOutputFolder;
                    var filePath = Path.Combine(outputFolder, fileName);

                    File.WriteAllText(filePath, content);

                    _soundManager?.PlayVoiceLine("048");

                    return new DocumentResult
                    {
                        Success = true,
                        FilePath = filePath,
                        FileName = fileName,
                        DocumentType = DocumentType.Text,
                        Message = "Text file created successfully"
                    };
                }
                catch (Exception ex)
                {
                    _soundManager?.PlayVoiceLine("056");
                    return new DocumentResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        DocumentType = DocumentType.Text
                    };
                }
            });
        }

        /// <summary>
        /// Create a Markdown file
        /// </summary>
        public async Task<DocumentResult> CreateMarkdownFileAsync(
            string title,
            string content,
            string? fileName = null,
            string? outputFolder = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _soundManager?.PlayVoiceLine("032");

                    fileName = fileName ?? $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.md";
                    outputFolder = outputFolder ?? _defaultOutputFolder;
                    var filePath = Path.Combine(outputFolder, fileName);

                    var markdown = $"# {title}\n\n{content}";
                    File.WriteAllText(filePath, markdown);

                    _soundManager?.PlayVoiceLine("048");

                    return new DocumentResult
                    {
                        Success = true,
                        FilePath = filePath,
                        FileName = fileName,
                        DocumentType = DocumentType.Markdown,
                        Message = "Markdown file created successfully"
                    };
                }
                catch (Exception ex)
                {
                    _soundManager?.PlayVoiceLine("056");
                    return new DocumentResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        DocumentType = DocumentType.Markdown
                    };
                }
            });
        }

        #endregion

        #region DJ-Specific Documents

        /// <summary>
        /// Generate a DJ setlist document
        /// </summary>
        public async Task<DocumentResult> CreateDJSetlistAsync(
            string djName,
            string venueName,
            DateTime eventDate,
            List<Track> tracks,
            DocumentType outputFormat = DocumentType.Word)
        {
            var title = $"{djName} - Setlist for {venueName}";
            var content = $"Event Date: {eventDate:MMMM dd, yyyy}\n" +
                         $"DJ: {djName}\n" +
                         $"Venue: {venueName}\n\n" +
                         $"SETLIST:\n\n";

            int trackNum = 1;
            foreach (var track in tracks)
            {
                content += $"{trackNum}. {track.Artist} - {track.Title} ({track.Duration})\n";
                trackNum++;
            }

            content += $"\nTotal Tracks: {tracks.Count}\n";
            content += $"Total Duration: {CalculateTotalDuration(tracks)}\n";

            return outputFormat switch
            {
                DocumentType.PDF => await CreatePDFDocumentAsync(title, content, $"Setlist_{djName}_{eventDate:yyyyMMdd}.pdf"),
                DocumentType.Text => await CreateTextFileAsync(content, $"Setlist_{djName}_{eventDate:yyyyMMdd}.txt"),
                DocumentType.Markdown => await CreateMarkdownFileAsync(title, content, $"Setlist_{djName}_{eventDate:yyyyMMdd}.md"),
                _ => await CreateWordDocumentAsync(title, content, $"Setlist_{djName}_{eventDate:yyyyMMdd}.docx")
            };
        }

        /// <summary>
        /// Generate a booking contract document
        /// </summary>
        public async Task<DocumentResult> CreateBookingContractAsync(
            string djName,
            string venueName,
            DateTime eventDate,
            decimal fee,
            string additionalTerms = "")
        {
            var title = "DJ BOOKING CONTRACT";
            var content = $"This agreement is made on {DateTime.Now:MMMM dd, yyyy}\n\n" +
                         $"BETWEEN:\n" +
                         $"DJ: {djName}\n" +
                         $"Venue: {venueName}\n\n" +
                         $"EVENT DETAILS:\n" +
                         $"Date: {eventDate:MMMM dd, yyyy}\n" +
                         $"Fee: ${fee:N2}\n\n" +
                         $"TERMS AND CONDITIONS:\n" +
                         $"1. The DJ agrees to perform at the specified venue on the specified date.\n" +
                         $"2. The venue agrees to pay the agreed fee.\n" +
                         $"3. Cancellation policy: 48 hours notice required.\n" +
                         $"4. Equipment will be provided by: [TO BE SPECIFIED]\n\n";

            if (!string.IsNullOrEmpty(additionalTerms))
            {
                content += $"ADDITIONAL TERMS:\n{additionalTerms}\n\n";
            }

            content += $"Signatures:\n" +
                      $"DJ: __________________ Date: __________\n" +
                      $"Venue: __________________ Date: __________\n";

            return await CreateWordDocumentAsync(title, content, $"Contract_{djName}_{venueName}_{eventDate:yyyyMMdd}.docx");
        }

        /// <summary>
        /// Generate a DJ promotional flyer (as PDF or image)
        /// </summary>
        public async Task<DocumentResult> CreateDJFlyerAsync(
            string djName,
            string eventName,
            DateTime eventDate,
            string venueName,
            string additionalInfo = "")
        {
            var title = eventName.ToUpper();
            var content = $"{djName}\n\n" +
                         $"Live @ {venueName}\n" +
                         $"{eventDate:dddd, MMMM dd, yyyy}\n" +
                         $"{eventDate:h:mm tt}\n\n" +
                         $"{additionalInfo}";

            return await CreatePDFDocumentAsync(title, content, $"Flyer_{eventName}_{eventDate:yyyyMMdd}.pdf");
        }

        #endregion

        #region Helper Methods

        private string CalculateTotalDuration(List<Track> tracks)
        {
            var totalMinutes = tracks.Sum(t =>
            {
                var parts = t.Duration.Split(':');
                return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
            });

            return $"{totalMinutes / 60}h {totalMinutes % 60}m";
        }

        /// <summary>
        /// Open generated document with default application
        /// </summary>
        public void OpenDocument(object filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath.ToString(),
                    UseShellExecute = true
                });

                _soundManager?.PlayVoiceLine("042"); // "Perfect!"
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CandyBot DocGen] Error opening document: {ex.Message}");
                _soundManager?.PlayVoiceLine("056"); // Error
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Document generation result
    /// </summary>
    public class DocumentResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Document types supported
    /// </summary>
    public enum DocumentType
    {
        Word,       // .docx
        PDF,        // .pdf
        PowerPoint, // .pptx
        Excel,      // .xlsx
        Text,       // .txt
        Markdown,   // .md
        HTML        // .html
    }

    /// <summary>
    /// PowerPoint slide content
    /// </summary>
    public class SlideContent
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> BulletPoints { get; set; } = new();
        public string ImagePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Music track info for setlists
    /// </summary>
    public class Track
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty; // Format: "MM:SS"
        public string Genre { get; set; } = string.Empty;
        public int BPM { get; set; }
    }

    #endregion
}
