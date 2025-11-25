# CandyBot Tutorial Configuration

## Tutorial Video Locations

The tutorial system expects video files to be located in the `CandyBot_Training_Guides` folder relative to the application directory.

### Expected Path Structure:
```
Application Root/
??? DJBookingSystem.exe
??? ../CandyBot_Training_Guides/
    ??? Tutorial_Users.mp4     (167 MB) - For all users
    ??? Tutorial_Admin.mp4     (212 MB) - For Managers & SysAdmins only
```

### Video Playback Order:
1. **Tutorial_Users.mp4** - Played for all new users
2. **Tutorial_Admin.mp4** - Played ONLY for users with Manager or SysAdmin role

### Tutorial Behavior:
- ? **Mandatory** - Cannot be skipped or closed until completed
- ? **Full Screen** - Covers entire screen
- ? **Blocks Alt+F4** and Escape key
- ? **Auto-marks as seen** after completion
- ? **One-time only** - Users see it once, then it's marked as seen in database

### Admin Controls:
Admins can control tutorial status in **Users Management** panel:
- **Checked (?)** = User has seen tutorial
- **Unchecked ( )** = User will see mandatory tutorial on next login

### Database Field:
```json
{
  "hasSeenTutorial": true/false
}
```

### Implementation Files:
- `MainWindow.Tutorial.cs` - Auto-launch logic
- `VideoTutorialWindow.xaml` - Tutorial player UI
- `VideoTutorialWindow.xaml.cs` - Multi-video support with mandatory mode
- `Models\User.cs` - HasSeenTutorial property
- `Views\UsersView.xaml` - Admin checkbox to view/edit status

### Troubleshooting:

#### Videos Not Found
If videos are not found, the system will:
1. Show a warning message
2. Mark tutorial as seen (to prevent infinite loops)
3. Allow user to continue

#### Path Issues
The application resolves the path as:
```csharp
Path.Combine(AppDirectory, "..", "..", "CandyBot_Training_Guides")
```

This typically resolves to:
- **Debug**: `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\CandyBot_Training_Guides\`
- **Release**: `[InstallDir]\CandyBot_Training_Guides\`

### CandyBot Welcome Greeting:
After tutorial completion (or if already seen), CandyBot will:
- ? Play voice greeting: "Welcome back, [User Name]!"
- ? Show welcome animation
- ? Continue playing on every login (not just first time)

### Manual Tutorial Access:
Users can manually replay tutorials from:
- **Menu** ? **Tutorial** (pink menu item)
- This plays in optional mode (can be skipped)
