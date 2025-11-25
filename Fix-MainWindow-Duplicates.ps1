# Fix MainWindow.xaml duplicate attributes
$xamlPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\MainWindow.xaml"

# Read the file
$content = Get-Content $xamlPath -Raw

# Fix duplicate ImageBrush (remove the first one with New-BG.png)
$content = $content -replace '<ImageBrush ImageSource="New-BG\.png" Stretch="UniformToFill" Opacity="0\.8"/>\s*<ImageBrush', '<ImageBrush'

# Fix CandyBot duplicates - remove the duplicate comment line
$content = $content -replace '<!-- CandyBot Avatar Overlay - positioned by user under Live Status -->\s*<!-- CandyBot Avatar Overlay - Draggable anywhere in the app \(covers all rows\) -->', '<!-- CandyBot Avatar Overlay - Draggable anywhere in the app (covers all rows) -->'

# Fix duplicate Grid.Row attributes (remove Grid.Row="4", keep Grid.Row="0")
$content = $content -replace 'Grid\.Row="4"\s*Grid\.Row="0"', 'Grid.Row="0"'

# Fix duplicate Panel.ZIndex and remove AvatarClicked
$content = $content -replace 'Panel\.ZIndex="100"\s*AvatarClicked="CandyBot_Click"\s*Panel\.ZIndex="999"', 'Panel.ZIndex="999"'

# Write the fixed content back
Set-Content $xamlPath $content -NoNewline

Write-Host "? Fixed MainWindow.xaml duplicates!" -ForegroundColor Green
Write-Host "Changes made:" -ForegroundColor Cyan
Write-Host "  - Removed duplicate ImageBrush" -ForegroundColor Yellow
Write-Host "  - Removed duplicate Grid.Row" -ForegroundColor Yellow  
Write-Host "  - Removed duplicate Panel.ZIndex" -ForegroundColor Yellow
Write-Host "  - Removed invalid AvatarClicked event" -ForegroundColor Yellow
