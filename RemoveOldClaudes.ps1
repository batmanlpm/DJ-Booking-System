# Script to remove old ConnectionIndicator controls

$content = Get-Content "MainWindow.xaml" -Raw

# Find and remove the Claude001Monitor section
$claude001Start = $content.IndexOf('<!-- ?? CLAUDE001')
$claude001End = $content.IndexOf('Panel.ZIndex="1000"/>', $claude001Start) + 22

# Find and remove the Claude002Monitor section  
$claude002Start = $content.IndexOf('<!-- ?? CLAUDE002')
$claude002End = $content.IndexOf('Panel.ZIndex="1000"/>', $claude002Start) + 22

Write-Host "Claude001 found at: $claude001Start to $claude001End"
Write-Host "Claude002 found at: $claude002Start to $claude002End"

# Remove Claude002 first (it's later in the file)
if ($claude002Start -gt 0) {
    $before = $content.Substring(0, $claude002Start)
    $after = $content.Substring($claude002End)
    $content = $before + $after
    Write-Host "Removed Claude002Monitor"
}

# Then remove Claude001
$content = Get-Content "MainWindow.xaml" -Raw
$claude001Start = $content.IndexOf('<!-- ?? CLAUDE001')
if ($claude001Start -gt 0) {
    $claude001End = $content.IndexOf('Panel.ZIndex="1000"/>', $claude001Start) + 22
    $before = $content.Substring(0, $claude001Start)
    $after = $content.Substring($claude001End)
    $content = $before + $after
    Write-Host "Removed Claude001Monitor"
}

# Save
$content | Out-File "MainWindow.xaml" -Encoding UTF8 -NoNewline

Write-Host "Done! Old Claude indicators removed."
