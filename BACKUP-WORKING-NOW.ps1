# EMERGENCY BACKUP SCRIPT - WORKING VERSION!
# Run this immediately!

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$source = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
$backupRoot = "K:\Customer Data\LPM\DJ_Booking\BACKUPS_WORKING"

Write-Host "========================================" -ForegroundColor Green
Write-Host "?? EMERGENCY BACKUP - APP IS WORKING!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Create backup directory
New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null

# Backup 1: FULL COPY (everything)
Write-Host "Backup 1: Full project backup..." -ForegroundColor Yellow
$backup1 = "$backupRoot\WORKING_$timestamp`_FULL"
Copy-Item -Path $source -Destination $backup1 -Recurse -Force
Write-Host "  ? DONE: $backup1" -ForegroundColor Green

# Backup 2: Source only (no bin/obj)
Write-Host ""
Write-Host "Backup 2: Source code only..." -ForegroundColor Yellow
$backup2 = "$backupRoot\WORKING_$timestamp`_SOURCE"
robocopy $source $backup2 /E /XD bin obj .vs node_modules packages /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null
Write-Host "  ? DONE: $backup2" -ForegroundColor Green

# Create info file
$info = @"
===========================================
? WORKING VERSION BACKUP
===========================================

Timestamp: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Version: 1.2.5.0
Status: ? WORKING - APP STARTS AND RUNS!

===========================================
WHAT WAS FIXED TO MAKE IT WORK:
===========================================

1. Assembly Version (1.2.4 ? 1.2.5)
2. Certificate pinning disabled
3. Auto-update URL fixed
4. File corruption repaired (Claude rebuilt 17 files)
5. Missing CandyBot_Click handler restored
6. Build cache cleaned

===========================================
BACKUPS CREATED:
===========================================

Full:   $backup1
Source: $backup2

===========================================
TO RESTORE:
===========================================

1. Delete current project folder
2. Copy backup folder back
3. Rename to "Fallen-Collective-Booking"
4. Open in Visual Studio
5. Build and run

===========================================
"@

$info | Out-File "$backupRoot\BACKUP_INFO_$timestamp.txt" -Encoding UTF8

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "? BACKUP COMPLETE!" -ForegroundColor Green  
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Backups saved to:" -ForegroundColor Cyan
Write-Host "  $backupRoot" -ForegroundColor White
Write-Host ""
Write-Host "YOU'RE SAFE NOW! ??" -ForegroundColor Green
