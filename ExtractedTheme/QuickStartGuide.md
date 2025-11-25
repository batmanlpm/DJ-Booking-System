# Quick Start Guide - DJ Booking Theme Pack

## 🚀 Get Started in 5 Minutes

### Step 1: Add Theme to Your Project (30 seconds)

1. Copy `DJBookingTheme.xaml` to your WPF project
2. Place it in a `Themes` folder (create if needed)

### Step 2: Reference Theme in App.xaml (1 minute)

```xml
<Application x:Class="YourApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/DJBookingTheme.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### Step 3: Create Your First Themed Window (3 minutes)

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="My DJ App" 
        Height="600" 
        Width="800"
        Style="{StaticResource PrimaryWindowStyle}">
    
    <Grid>
        <!-- Space Background -->
        <Grid.Background>
            <StaticResource ResourceKey="SpaceBackgroundBrush"/>
        </Grid.Background>
        
        <!-- Main Content -->
        <Border Style="{StaticResource PanelBorderStyle}"
                Width="600"
                VerticalAlignment="Center">
            <StackPanel>
                <!-- Title -->
                <TextBlock Text="🎧 Welcome to DJ Booking 🎧"
                           Style="{StaticResource TitleTextBlockStyle}"
                           Margin="0,0,0,30"/>
                
                <!-- Form -->
                <Label Content="Your Name:" 
                       Style="{StaticResource PrimaryLabelStyle}"/>
                <TextBox Style="{StaticResource PrimaryTextBoxStyle}"
                         Margin="0,0,0,20"/>
                
                <Label Content="Select Venue:" 
                       Style="{StaticResource PrimaryLabelStyle}"/>
                <ComboBox Style="{StaticResource PrimaryComboBoxStyle}"
                          Margin="0,0,0,30">
                    <ComboBoxItem Content="Club Neon"/>
                    <ComboBoxItem Content="Space Station"/>
                    <ComboBoxItem Content="Cyber Lounge"/>
                </ComboBox>
                
                <!-- Button -->
                <Button Content="🎵 Book Now 🎵"
                        Style="{StaticResource PrimaryButtonStyle}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

### Step 4: Run Your App! ✨

Press F5 and see your themed application in action!

## 📚 Most Used Styles

### Buttons
```xml
<Button Style="{StaticResource PrimaryButtonStyle}"/>      <!-- Green button -->
<Button Style="{StaticResource DangerButtonStyle}"/>       <!-- Red button -->
<Button Style="{StaticResource WarningButtonStyle}"/>      <!-- Yellow button -->
```

### Text
```xml
<TextBlock Style="{StaticResource TitleTextBlockStyle}"/>      <!-- Large title -->
<TextBlock Style="{StaticResource SubtitleTextBlockStyle}"/>   <!-- Subtitle -->
<TextBlock Style="{StaticResource PrimaryTextBlockStyle}"/>    <!-- Body text -->
```

### Inputs
```xml
<TextBox Style="{StaticResource PrimaryTextBoxStyle}"/>
<PasswordBox Style="{StaticResource PrimaryPasswordBoxStyle}"/>
<ComboBox Style="{StaticResource PrimaryComboBoxStyle}"/>
```

### Containers
```xml
<Border Style="{StaticResource PanelBorderStyle}"/>    <!-- Main panels -->
<Border Style="{StaticResource CardBorderStyle}"/>     <!-- Cards -->
<Border Style="{StaticResource FormBorderStyle}"/>     <!-- Forms -->
```

### Messages
```xml
<Border Style="{StaticResource SuccessMessageStyle}"/>  <!-- Success -->
<Border Style="{StaticResource ErrorMessageStyle}"/>    <!-- Error -->
<Border Style="{StaticResource InfoMessageStyle}"/>     <!-- Info -->
```

## 🎨 Color Quick Reference

```xml
<!-- Use these for text colors -->
Foreground="{StaticResource NeonGreenBrush}"      <!-- Primary green -->
Foreground="{StaticResource ErrorRedBrush}"       <!-- Errors -->
Foreground="{StaticResource InfoCyanBrush}"       <!-- Info -->
Foreground="{StaticResource WarningOrangeBrush}"  <!-- Warnings -->

<!-- Use these for backgrounds -->
Background="{StaticResource SpaceBackgroundBrush}"   <!-- Main background -->
Background="{StaticResource PanelBackgroundBrush}"   <!-- Panels -->
Background="{StaticResource CardBackgroundBrush}"    <!-- Cards -->
```

## 💡 Pro Tips

1. **Always use StaticResource**: `Style="{StaticResource PrimaryButtonStyle}"`
2. **Space background first**: Apply `SpaceBackgroundBrush` to your main Grid
3. **Consistent spacing**: Use `Margin="{StaticResource MediumMargin}"`
4. **Semantic colors**: Use `SuccessGreenBrush` for success, `ErrorRedBrush` for errors
5. **Check examples**: Open `UsageExamples.xaml` for more complex scenarios

## 🆘 Common Issues

**Styles not applying?**
- Check that theme is merged in App.xaml
- Verify the Source path is correct

**Colors look wrong?**
- Ensure you're using StaticResource, not DynamicResource
- Check that resource keys are spelled correctly

**Need more help?**
- See full README.md for detailed documentation
- Check UsageExamples.xaml for working examples

---

**That's it! You're ready to build amazing DJ-themed applications! 🎉**