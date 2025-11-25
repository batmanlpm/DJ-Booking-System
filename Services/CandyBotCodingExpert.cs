using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Candy-Bot Coding Expert Service
    /// Makes Candy an expert in ALL programming languages and development
    /// </summary>
    public class CandyBotCodingExpert
    {
        private readonly CandyBotTextToSpeech? _tts;
        private Dictionary<string, LanguageExpertise> _languageKnowledge = new();

        public CandyBotCodingExpert(CandyBotTextToSpeech? tts = null)
        {
            _tts = tts;
            InitializeLanguageKnowledge();
        }

        /// <summary>
        /// Initialize Candy's coding knowledge base
        /// </summary>
        private void InitializeLanguageKnowledge()
        {
            _languageKnowledge = new Dictionary<string, LanguageExpertise>
            {
                // Web Development
                ["html"] = new LanguageExpertise
                {
                    Name = "HTML",
                    FileExtensions = new[] { ".html", ".htm" },
                    Category = "Web",
                    Description = "Markup for web pages",
                    CommonPatterns = new[] { "<!DOCTYPE html>", "<div>", "<script>" }
                },
                ["css"] = new LanguageExpertise
                {
                    Name = "CSS",
                    FileExtensions = new[] { ".css" },
                    Category = "Web",
                    Description = "Styling for web pages",
                    CommonPatterns = new[] { "body {", ".class {", "#id {" }
                },
                ["javascript"] = new LanguageExpertise
                {
                    Name = "JavaScript",
                    FileExtensions = new[] { ".js", ".jsx" },
                    Category = "Web",
                    Description = "Client-side scripting",
                    CommonPatterns = new[] { "function", "const", "let", "=>" }
                },
                ["typescript"] = new LanguageExpertise
                {
                    Name = "TypeScript",
                    FileExtensions = new[] { ".ts", ".tsx" },
                    Category = "Web",
                    Description = "Typed JavaScript",
                    CommonPatterns = new[] { "interface", "type", "enum" }
                },

                // Backend Languages
                ["csharp"] = new LanguageExpertise
                {
                    Name = "C#",
                    FileExtensions = new[] { ".cs" },
                    Category = "Backend",
                    Description = ".NET programming",
                    CommonPatterns = new[] { "using", "namespace", "class", "public" }
                },
                ["python"] = new LanguageExpertise
                {
                    Name = "Python",
                    FileExtensions = new[] { ".py" },
                    Category = "Backend",
                    Description = "General-purpose scripting",
                    CommonPatterns = new[] { "def", "import", "class", "if __name__" }
                },
                ["java"] = new LanguageExpertise
                {
                    Name = "Java",
                    FileExtensions = new[] { ".java" },
                    Category = "Backend",
                    Description = "Enterprise programming",
                    CommonPatterns = new[] { "public class", "import", "package" }
                },
                ["php"] = new LanguageExpertise
                {
                    Name = "PHP",
                    FileExtensions = new[] { ".php" },
                    Category = "Backend",
                    Description = "Server-side scripting",
                    CommonPatterns = new[] { "<?php", "function", "$" }
                },

                // Systems Programming
                ["cpp"] = new LanguageExpertise
                {
                    Name = "C++",
                    FileExtensions = new[] { ".cpp", ".h", ".hpp" },
                    Category = "Systems",
                    Description = "High-performance programming",
                    CommonPatterns = new[] { "#include", "int main(", "std::" }
                },
                ["rust"] = new LanguageExpertise
                {
                    Name = "Rust",
                    FileExtensions = new[] { ".rs" },
                    Category = "Systems",
                    Description = "Memory-safe systems programming",
                    CommonPatterns = new[] { "fn main(", "let mut", "impl" }
                },

                // Markup & Data
                ["xml"] = new LanguageExpertise
                {
                    Name = "XML/XAML",
                    FileExtensions = new[] { ".xml", ".xaml" },
                    Category = "Markup",
                    Description = "Data structure markup",
                    CommonPatterns = new[] { "<?xml", "<Window", "<Grid>" }
                },
                ["json"] = new LanguageExpertise
                {
                    Name = "JSON",
                    FileExtensions = new[] { ".json" },
                    Category = "Data",
                    Description = "Data interchange format",
                    CommonPatterns = new[] { "{", "\":", "[" }
                },
                ["yaml"] = new LanguageExpertise
                {
                    Name = "YAML",
                    FileExtensions = new[] { ".yml", ".yaml" },
                    Category = "Data",
                    Description = "Human-readable data format",
                    CommonPatterns = new[] { "---", "  -", ":" }
                },

                // Database
                ["sql"] = new LanguageExpertise
                {
                    Name = "SQL",
                    FileExtensions = new[] { ".sql" },
                    Category = "Database",
                    Description = "Database queries",
                    CommonPatterns = new[] { "SELECT", "FROM", "WHERE", "JOIN" }
                }
            };
        }

        /// <summary>
        /// Get Candy's expert system prompt for coding
        /// This makes her an expert in ALL programming!
        /// </summary>
        public string GetCodingExpertPrompt()
        {
            return @"
🍬 CANDY-BOT CODING EXPERT MODE 🍬

You are Candy-Bot, an expert-level software developer with deep knowledge in:

**WEB DEVELOPMENT:**
- HTML5, CSS3, JavaScript (ES6+), TypeScript
- React, Vue, Angular, Svelte
- Node.js, Express, Next.js
- Responsive design, accessibility (a11y)
- Web APIs, fetch, async/await
- Webpack, Vite, build tools

**BACKEND & LANGUAGES:**
- C# (.NET Core, ASP.NET, WPF, XAML)
- Python (Django, Flask, FastAPI)
- Java (Spring Boot, Hibernate)
- PHP (Laravel, WordPress)
- Go, Rust (systems programming)

**DATABASES:**
- SQL (PostgreSQL, MySQL, SQL Server)
- NoSQL (MongoDB, CosmosDB, Redis)
- ORMs (Entity Framework, Sequelize)

**MOBILE:**
- React Native, Flutter
- iOS (Swift), Android (Kotlin)
- Progressive Web Apps (PWA)

**DEVOPS & TOOLS:**
- Git, GitHub, Azure DevOps
- Docker, Kubernetes
- CI/CD pipelines
- Azure, AWS, Google Cloud

**YOUR CODING STYLE:**
1. Write clean, readable code with proper formatting
2. Include helpful comments explaining complex logic
3. Follow best practices and design patterns
4. Consider performance and security
5. Provide complete, working examples
6. Explain your code clearly
7. Suggest improvements and alternatives

**WHEN HELPING WITH CODE:**
- Always provide complete, runnable code
- Include necessary imports/using statements
- Add error handling
- Format code properly with correct indentation
- Explain what the code does
- Point out potential issues
- Suggest optimizations

**YOUR PERSONALITY:**
- Enthusiastic about coding
- Patient when explaining concepts
- Encouraging with beginners
- Detail-oriented but not overwhelming
- Use emojis occasionally to stay friendly 🍬

Remember: You can code ANYTHING in ANY language!
";
        }

        /// <summary>
        /// Detect programming language from code snippet
        /// </summary>
        public string DetectLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "unknown";

            code = code.Trim();

            // Check for distinctive patterns
            foreach (var lang in _languageKnowledge)
            {
                foreach (var pattern in lang.Value.CommonPatterns)
                {
                    if (code.Contains(pattern))
                    {
                        return lang.Key;
                    }
                }
            }

            // Check file-like syntax
            if (code.StartsWith("<!DOCTYPE") || code.StartsWith("<html"))
                return "html";
            if (code.StartsWith("<?xml") || code.StartsWith("<Window"))
                return "xml";
            if (code.StartsWith("{") && code.Contains("\":"))
                return "json";
            if (code.Contains("using System") || code.Contains("namespace"))
                return "csharp";
            if (code.Contains("def ") && code.Contains(":"))
                return "python";

            return "unknown";
        }

        /// <summary>
        /// Generate code example based on request
        /// </summary>
        public async Task<CodeGenerationResult> GenerateCodeAsync(CodeRequest request)
        {
            Debug.WriteLine($"[Candy Coding] Generating {request.Language} code for: {request.Description}");

            var result = new CodeGenerationResult
            {
                Language = request.Language,
                Description = request.Description,
                Code = GenerateCodeTemplate(request),
                Explanation = $"Here's a {request.Language} implementation for: {request.Description}"
            };

            // Candy announces what she's doing
            if (_tts != null)
            {
                await _tts.SpeakAsync($"I've generated a {request.Language} code example for you!");
            }

            return result;
        }

        /// <summary>
        /// Generate code template based on language and type
        /// </summary>
        private string GenerateCodeTemplate(CodeRequest request)
        {
            var lang = request.Language.ToLower();

            // Template generators for different scenarios
            return lang switch
            {
                "csharp" => GenerateCSharpTemplate(request),
                "python" => GeneratePythonTemplate(request),
                "javascript" => GenerateJavaScriptTemplate(request),
                "html" => GenerateHTMLTemplate(request),
                "css" => GenerateCSSTemplate(request),
                "sql" => GenerateSQLTemplate(request),
                _ => GenerateGenericTemplate(request)
            };
        }

        #region Language-Specific Templates

        private string GenerateCSharpTemplate(CodeRequest request)
        {
            if (request.Type == "class")
            {
                return $@"using System;

namespace YourNamespace
{{
    /// <summary>
    /// {request.Description}
    /// </summary>
    public class {request.ClassName ?? "MyClass"}
    {{
        // Properties
        public string Name {{ get; set; }}
        
        // Constructor
        public {request.ClassName ?? "MyClass"}()
        {{
            // Initialize
        }}
        
        // Methods
        public void DoSomething()
        {{
            Console.WriteLine(""Hello from Candy-Bot!"");
        }}
    }}
}}";
            }
            else if (request.Type == "method")
            {
                return $@"/// <summary>
/// {request.Description}
/// </summary>
public void {request.MethodName ?? "MyMethod"}()
{{
    // Your code here
    Console.WriteLine(""Method called!"");
}}";
            }

            return "// C# code template";
        }

        private string GeneratePythonTemplate(CodeRequest request)
        {
            if (request.Type == "class")
            {
                return $@"class {request.ClassName ?? "MyClass"}:
    """"""{request.Description}""""""
    
    def __init__(self):
        """"""Initialize the class""""""
        self.name = """"
    
    def do_something(self):
        """"""Perform an action""""""
        print(""Hello from Candy-Bot!"")

# Usage
if __name__ == ""__main__"":
    obj = {request.ClassName ?? "MyClass"}()
    obj.do_something()
";
            }

            return "# Python code template";
        }

        private string GenerateJavaScriptTemplate(CodeRequest request)
        {
            if (request.Type == "function")
            {
                return $@"/**
 * {request.Description}
 */
function {request.MethodName ?? "myFunction"}() {{
    console.log('Hello from Candy-Bot!');
}}

// Arrow function alternative
const {request.MethodName ?? "myFunction"}Alt = () => {{
    console.log('Hello from Candy-Bot!');
}};";
            }
            else if (request.Type == "class")
            {
                return $@"/**
 * {request.Description}
 */
class {request.ClassName ?? "MyClass"} {{
    constructor() {{
        this.name = '';
    }}
    
    doSomething() {{
        console.log('Hello from Candy-Bot!');
    }}
}}

// Usage
const obj = new {request.ClassName ?? "MyClass"}();
obj.doSomething();";
            }

            return "// JavaScript code template";
        }

        private string GenerateHTMLTemplate(CodeRequest request)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{request.Description ?? "Candy-Bot Generated Page"}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }}
        .container {{
            max-width: 800px;
            margin: 0 auto;
            background: rgba(255, 255, 255, 0.1);
            padding: 30px;
            border-radius: 10px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>🍬 {request.Description ?? "Hello from Candy-Bot!"}</h1>
        <p>This page was generated by Candy-Bot Coding Expert!</p>
    </div>
    
    <script>
        console.log('Candy-Bot says hi! 🍬');
    </script>
</body>
</html>";
        }

        private string GenerateCSSTemplate(CodeRequest request)
        {
            return $@"/* {request.Description} */
/* Created by Candy-Bot 🍬 */

:root {{
    --primary-color: #667eea;
    --secondary-color: #764ba2;
    --text-color: #333;
}}

* {{
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}}

body {{
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--secondary-color) 100%);
    color: var(--text-color);
}}

.container {{
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}}

.button {{
    background: var(--primary-color);
    color: white;
    padding: 10px 20px;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    transition: all 0.3s ease;
}}

.button:hover {{
    transform: translateY(-2px);
    box-shadow: 0 5px 15px rgba(0,0,0,0.3);
}}";
        }

        private string GenerateSQLTemplate(CodeRequest request)
        {
            return $@"-- {request.Description}
-- Created by Candy-Bot 🍬

-- Create table
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Insert sample data
INSERT INTO Users (Username, Email)
VALUES ('CandyBot', 'candy@bot.com');

-- Select data
SELECT * FROM Users
WHERE Username = 'CandyBot';

-- Update data
UPDATE Users
SET Email = 'new@email.com'
WHERE Username = 'CandyBot';";
        }

        private string GenerateGenericTemplate(CodeRequest request)
        {
            return $@"// {request.Description}
// Language: {request.Language}
// Created by Candy-Bot 🍬

// Your code here...
";
        }

        #endregion

        /// <summary>
        /// Analyze code and provide suggestions
        /// </summary>
        public async Task<CodeAnalysisResult> AnalyzeCodeAsync(string code)
        {
            var language = DetectLanguage(code);
            var result = new CodeAnalysisResult
            {
                Language = language,
                CodeLength = code.Length,
                LineCount = code.Split('\n').Length
            };

            // Basic analysis
            result.Suggestions = new List<string>();

            // Check for common issues
            if (!code.Contains("//") && !code.Contains("/*") && language != "python")
            {
                result.Suggestions.Add("💡 Consider adding comments to explain your code");
            }

            if (code.Contains("catch { }"))
            {
                result.Suggestions.Add("⚠️ Empty catch blocks can hide errors - add logging");
            }

            if (code.Contains("TODO") || code.Contains("FIXME"))
            {
                result.Suggestions.Add("📝 You have TODO/FIXME notes to address");
            }

            if (_tts != null)
            {
                await _tts.SpeakAsync($"I've analyzed your {language} code and found {result.Suggestions.Count} suggestions.");
            }

            return result;
        }

        /// <summary>
        /// Get all supported languages
        /// </summary>
        public List<LanguageExpertise> GetSupportedLanguages()
        {
            return _languageKnowledge.Values.ToList();
        }
    }

    #region Supporting Classes

    public class LanguageExpertise
    {
        public string Name { get; set; } = string.Empty;
        public string[] FileExtensions { get; set; } = Array.Empty<string>();
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] CommonPatterns { get; set; } = Array.Empty<string>();
    }

    public class CodeRequest
    {
        public string Language { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "class", "method", "function", "page"
        public string ClassName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
    }

    public class CodeGenerationResult
    {
        public string Language { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public class CodeAnalysisResult
    {
        public string Language { get; set; } = string.Empty;
        public int CodeLength { get; set; }
        public int LineCount { get; set; }
        public List<string> Suggestions { get; set; } = new();
    }

    #endregion
}
