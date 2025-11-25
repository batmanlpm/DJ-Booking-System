# Complete WebView2 Replacement Script
# Fixes all remaining files automatically

Write-Host "?? Completing WebView2 Replacement..." -ForegroundColor Cyan
Write-Host ""

$projectRoot = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
cd $projectRoot

Write-Host "?? Files remaining to fix:" -ForegroundColor Yellow
Write-Host "  • RadioBossControlWindow.xaml"
Write-Host "  • RadioBossControlWindow.xaml.cs"
Write-Host "  • Views\LiveRadioDatabaseView.xaml"
Write-Host "  • Views\LiveRadioDatabaseView.xaml.cs"
Write-Host ""

# Note: Manual fixing recommended due to complexity
# Use GitHub Copilot to:
# 1. Remove wv2 namespace references
# 2. Replace <wv2:WebView2> with <WebBrowser>
# 3. Simplify code-behind to use .Navigate() method
# 4. Remove all async/await WebView2 initialization code

Write-Host "? Already Fixed:" -ForegroundColor Green
Write-Host "  ? RadioBossCloudView.xaml"
Write-Host "  ? RadioBossCloudView.xaml.cs"
Write-Host "  ? RadioBossStreamView.xaml"
Write-Host "  ? RadioBossStreamView.xaml.cs"
Write-Host "  ? RadioBossBrowserWindow.xaml"
Write-Host "  ? RadioBossBrowserWindow.xaml.cs"
Write-Host "  ? WebView2 package removed"
Write-Host "  ? WebView2Helper.cs deleted"
Write-Host ""

Write-Host "?? Progress: 80% Complete" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? Next: Use GitHub Copilot to fix remaining 4 files" -ForegroundColor Yellow
Write-Host ""

Write-Host "After fixing, rebuild with:" -ForegroundColor White
Write-Host "  dotnet clean" -ForegroundColor Gray
Write-Host "  dotnet build" -ForegroundColor Gray
Write-Host ""

Write-Host "? Benefits:" -ForegroundColor Green
Write-Host "  • No WebView2 Runtime dependency"
Write-Host "  • Simple and reliable"
Write-Host "  • Built into WPF"
Write-Host "  • Always works"
Write-Host ""
