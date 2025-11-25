# Fix all remaining BookingDate errors
$files = @(
    "EditBookingWindow.xaml.cs",
    "BookingDetailWindow.xaml.cs",
    "VenueDailyScheduleWindow.xaml.cs",
    "ViewModels\BookingViewModel.cs",
    "Views\CreateBookingWindow.xaml.cs",
    "Views\Bookings\BookingsView.xaml.cs",
    "Views\BookingsView.xaml.cs"
)

$root = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

foreach ($file in $files) {
    $path = Join-Path $root $file
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        
        # Replace all BookingDate usages
        $content = $content -replace '\.BookingDate\.ToString\([^\)]+\)', '.GetNextOccurrence(DateTime.Now).ToString("d")'
        $content = $content -replace '\.BookingDate\.Date', '.GetNextOccurrence(DateTime.Now).Date'
        $content = $content -replace 'booking\.BookingDate\s*=', '// booking.BookingDate = // REMOVED'
        $content = $content -replace '\.BookingDate\)', '.GetNextOccurrence(DateTime.Now))'
        
        $content | Set-Content $path -NoNewline
        Write-Host "Fixed: $file" -ForegroundColor Green
    }
}
