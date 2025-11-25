# Sign the installer with a code signing certificate
# NOTE: You need to purchase and install a code signing certificate first

param(
    [string]$CertificateThumbprint = "YOUR_CERT_THUMBPRINT_HERE",
    [string]$TimestampServer = "http://timestamp.digicert.com"
)

$installerPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\DJBookingSystem-Setup-v1.3.0.exe"

Write-Host "Signing installer with code signing certificate..." -ForegroundColor Cyan

# Sign the file
Set-AuthenticodeSignature -FilePath $installerPath `
    -Certificate (Get-Item "Cert:\CurrentUser\My\$CertificateThumbprint") `
    -TimestampServer $TimestampServer `
    -HashAlgorithm SHA256

Write-Host "? Installer signed successfully!" -ForegroundColor Green
Write-Host "SmartScreen warnings will no longer appear." -ForegroundColor Green
