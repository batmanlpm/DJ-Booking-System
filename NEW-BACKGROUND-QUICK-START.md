# ?? NEW BACKGROUND - QUICK START

## ? What Was Done
Added **New-BG.png** as the main background image for your DJ Booking System.

## ?? NEXT STEP: Place the Image File
Copy your **New-BG.png** file to:
```
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\New-BG.png
```

## ?? What It Does
- Shows the musical notes background behind all content
- Keeps your green theme and borders
- Works across all views (Bookings, Venues, Radio, etc.)
- 80% opacity so text stays readable

## ?? Quick Adjustments

### Make Background More/Less Visible
Edit `MainWindow.xaml` line 294:

```xaml
<!-- More subtle (50%) -->
<ImageBrush ImageSource="New-BG.png" Stretch="UniformToFill" Opacity="0.5"/>

<!-- Current (80%) -->
<ImageBrush ImageSource="New-BG.png" Stretch="UniformToFill" Opacity="0.8"/>

<!-- Full visibility (100%) -->
<ImageBrush ImageSource="New-BG.png" Stretch="UniformToFill" Opacity="1.0"/>
```

## ?? Test It
1. Place New-BG.png in project root
2. Close running app
3. Rebuild solution
4. Launch app - background should appear!

## ?? Your Preview
The code now matches your preview with:
- ? Musical notes floating
- ?? Blue gradient background  
- ? Green theme preserved
- ?? Professional DJ atmosphere

## ? Issues?
- **Image not showing?** Check file is named exactly "New-BG.png" in project root
- **Too dark/light?** Adjust Opacity value (0.0-1.0)
- **Wrong size?** Try different Stretch modes (UniformToFill, Uniform, Fill, None)
