# Restore Desktop Widget to Last Working State
Write-Host "Restoring CandyBotDesktopWidget.xaml.cs..." -ForegroundColor Yellow

# Get file from git
$content = git show HEAD:CandyBotDesktopWidget.xaml.cs

if ($content) {
    Set-Content -Path "CandyBotDesktopWidget.xaml.cs" -Value $content
    Write-Host "File restored successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to restore file from git" -ForegroundColor Red
}
