# ?? INTRO VIDEO SYSTEM

## Overview

The DJ Booking System now plays a random 8-second intro video after the splash screen and before the login window or main window appears.

---

## ?? File Location

**Intro Video Folder:**
```
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Fallen Intro\
```

**Expected Files:**
- `intro1.mp4` (or any .mp4, .avi, .wmv, .mov files)
- `intro2.mp4`
- `intro3.mp4`
- `intro4.mp4`

**Supported Formats:**
- `.mp4` (Recommended)
- `.avi`
- `.wmv`
- `.mov`

---

## ?? How It Works

### Startup Sequence:
```
1. Splash Screen (connection checks)
   ?
2. Random Intro Video (8 seconds) ? NEW
   ?
3. Login Window (if not auto-login)
   ?
4. Main Window
```

### Random Selection:
- **One random video** is selected from the `Fallen Intro` folder each time
- Different video may play each time the app starts
- If folder is empty or not found, intro is skipped

---

## ?? User Controls

**Skip Intro:**
- Press **ESC** key
- Press **SPACE** key
- Press **ENTER** key

**Auto-Skip:**
- Video automatically closes after playback completes (~8 seconds)

---

## ?? Implementation Details

### Files Created:

1. **`IntroVideoWindow.xaml`** - Full-screen video player window
2. **`IntroVideoWindow.xaml.cs`** - Video playback logic + random selection

### Modified Files:

3. **`App.xaml.cs`** - Added intro video playback after splash screen

---

## ?? IntroVideoWindow Features

### Video Playback:
- ? Full-screen display
- ? Maximized window
- ? Black background
- ? Stays on top
- ? Auto-play on load
- ? Auto-close on completion

### Random Selection:
```csharp
public static string? GetRandomIntroVideo()
{
    // Scans Fallen Intro folder
    // Filters for video files (.mp4, .avi, .wmv, .mov)
    // Returns random file path
    // Returns null if no videos found
}
```

### Skip Functionality:
- ESC key - Skip intro
- SPACE key - Skip intro
- ENTER key - Skip intro

---

## ?? Debugging

### Debug Output:
```
[IntroVideo] Selected: intro3.mp4 (from 4 videos)
[IntroVideo] Playing: intro3.mp4
[IntroVideo] Playback started
[IntroVideo] Video ended
```

### If Videos Not Found:
```
[IntroVideo] Intro folder not found: K:\Customer Data\...
[IntroVideo] No video files found in intro folder
?? No intro videos found - skipping intro
```

---

## ?? Adding/Removing Videos

### To Add Videos:
1. Place video files in: `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Fallen Intro\`
2. Use supported formats (.mp4, .avi, .wmv, .mov)
3. No code changes needed - automatically detected

### To Remove Videos:
1. Delete files from `Fallen Intro` folder
2. App will select from remaining videos
3. If all removed, intro is automatically skipped

### Recommended:
- **Format:** MP4 (H.264)
- **Resolution:** 1920x1080 (Full HD)
- **Duration:** 8 seconds
- **Size:** < 10 MB per video

---

## ?? Configuration

### Disable Intro Videos (Temporarily):
Delete or rename the `Fallen Intro` folder - intro will be skipped.

### Force Specific Video:
Modify `IntroVideoWindow.GetRandomIntroVideo()` to return a specific path instead of random selection.

### Change Folder Path:
Edit `IntroVideoWindow.xaml.cs` line 70:
```csharp
string introFolder = @"K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Fallen Intro";
```

---

## ?? Troubleshooting

### Video Not Playing:
1. **Check folder exists:**
   ```
   K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Fallen Intro\
   ```

2. **Check video format:**
   - Must be .mp4, .avi, .wmv, or .mov
   - Recommend MP4 for best compatibility

3. **Check codec:**
   - Windows Media Player should be able to play the file
   - H.264 codec recommended

4. **Check debug output:**
   - Run in Visual Studio (F5)
   - View Output window
   - Look for `[IntroVideo]` messages

### Video Stutters/Lags:
- Reduce video file size
- Use H.264 codec
- Lower resolution if needed

### Video Doesn't Auto-Close:
- Check duration (should be ~8 seconds)
- Press ESC to skip manually

---

## ?? Technical Details

### MediaElement Settings:
```csharp
LoadedBehavior = MediaState.Manual
UnloadedBehavior = MediaState.Close
Stretch = Stretch.Uniform
StretchDirection = StretchDirection.Both
```

### Window Settings:
```csharp
WindowStyle = WindowStyle.None
WindowState = WindowState.Maximized
ResizeMode = ResizeMode.NoResize
Topmost = True
Background = Black
```

---

## ? Features

? **Random Selection** - Different video each startup  
? **Auto-Play** - Starts immediately  
? **Auto-Close** - No user interaction needed  
? **Skip Enabled** - ESC/SPACE/ENTER to skip  
? **Full-Screen** - Maximized display  
? **Error Handling** - Gracefully skips if videos missing  
? **Debug Logging** - Track which video plays  

---

## ?? Future Enhancements (Optional)

### Possible Additions:
- **Volume control** - Mute/unmute option
- **Fade in/out** - Smooth transitions
- **Progress indicator** - Show remaining time
- **Skip button** - Visual UI element
- **Weighted random** - Prefer certain videos
- **Sequential playback** - Play all 4 in order

---

## ?? Example Folder Structure

```
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
??? Fallen Intro\
?   ??? intro1.mp4  (8 seconds, 1920x1080)
?   ??? intro2.mp4  (8 seconds, 1920x1080)
?   ??? intro3.mp4  (8 seconds, 1920x1080)
?   ??? intro4.mp4  (8 seconds, 1920x1080)
??? DJBookingSystem.exe
??? IntroVideoWindow.xaml
??? IntroVideoWindow.xaml.cs
```

---

## ?? Quick Test

1. **Add videos** to `Fallen Intro` folder
2. **Run app** (F5 in Visual Studio)
3. **Watch splash screen** complete
4. **Random intro plays** (8 seconds)
5. **Auto-closes** or press ESC to skip
6. **Login/MainWindow** appears

---

## ? Success Criteria

Intro system working when:
- ? Random video selected from folder
- ? Video plays full-screen
- ? Auto-closes after playback
- ? Can skip with ESC/SPACE/ENTER
- ? Gracefully skips if no videos found
- ? Different video each time

---

**Last Updated:** 2025-01-23  
**Version:** 1.2.5  
**Status:** ? Implemented
