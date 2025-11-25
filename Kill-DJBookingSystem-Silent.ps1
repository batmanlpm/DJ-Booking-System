# Silent Kill DJ Booking System
# No window, instant kill - perfect for hotkey

Get-Process -Name "DJBookingSystem" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "vshost*" -ErrorAction SilentlyContinue | Stop-Process -Force
exit
