' Silent Kill DJ Booking System - VBS Wrapper
' Runs PowerShell script completely hidden (no window flash)
' Perfect for keyboard hotkey assignment

Set WshShell = CreateObject("WScript.Shell")

' Get script directory
scriptDir = CreateObject("Scripting.FileSystemObject").GetParentFolderName(WScript.ScriptFullName)

' Run PowerShell script hidden
WshShell.Run "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & scriptDir & "\Kill-DJBookingSystem-Silent.ps1""", 0, False

' Exit immediately
Set WshShell = Nothing
