# Test-Minimal-WPF.ps1
# Creates a minimal WPF app to test if the basic framework works

$minimalApp = @'
using System;
using System.Windows;

namespace MinimalTest
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                MessageBox.Show("If you see this, WPF works!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                var app = new Application();
                var window = new Window
                {
                    Title = "Minimal Test",
                    Width = 400,
                    Height = 300,
                    Content = new System.Windows.Controls.TextBlock
                    {
                        Text = "WPF is working!",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 24
                    }
                };
                
                app.Run(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: {ex.Message}\n\nStack:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
'@

Write-Host "Creating minimal WPF test..." -ForegroundColor Cyan
$minimalApp | Out-File "MinimalTest.cs" -Encoding UTF8

Write-Host "Compiling..." -ForegroundColor Yellow
csc /target:winexe /r:"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.11\PresentationFramework.dll" /r:"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.11\PresentationCore.dll" /r:"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.11\WindowsBase.dll" /r:"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.11\System.Runtime.dll" MinimalTest.cs

if ($LASTEXITCODE -eq 0) {
    Write-Host "Running minimal test..." -ForegroundColor Green
    .\MinimalTest.exe
} else {
    Write-Host "Compilation failed!" -ForegroundColor Red
}
