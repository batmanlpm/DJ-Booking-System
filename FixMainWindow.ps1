# Script to remove duplicate venue cards from MainWindow.xaml

$content = Get-Content "MainWindow.xaml" -Raw

# Find the WrapPanel line
$wrapPanelStart = $content.IndexOf('<WrapPanel x:Name="VenuesWrapPanel"')

# Find where the next TabItem starts (Tab 4)
$nextTabStart = $content.IndexOf('<!-- Tab 4: AVAILABLE VENUES')

Write-Host "WrapPanel starts at position: $wrapPanelStart"
Write-Host "Next tab starts at position: $nextTabStart"

# Extract the section between WrapPanel and Tab 4
$beforeWrap = $content.Substring(0, $wrapPanelStart)
$afterTab = $content.Substring($nextTabStart)

# Create clean WrapPanel section
$cleanWrapPanel = @"
<WrapPanel x:Name="VenuesWrapPanel" Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
                                            <!-- Placeholder: No venues currently configured -->
                                            <Border Background="#0A0A0A" 
                                                    BorderBrush="#00FF00" 
                                                    BorderThickness="2" 
                                                    Padding="40" 
                                                    Margin="20"
                                                    CornerRadius="8">
                                                <StackPanel>
                                                    <TextBlock Text="?? No Venues Available"
                                                              FontSize="24"
                                                              FontWeight="Bold"
                                                              Foreground="#00FF00"
                                                              FontFamily="Consolas"
                                                              HorizontalAlignment="Center"
                                                              Margin="0,0,0,15"/>
                                                    <TextBlock Text="Venues will appear here when they register for Open Decks events."
                                                              FontSize="14"
                                                              Foreground="#888888"
                                                              FontFamily="Consolas"
                                                              TextWrapping="Wrap"
                                                              TextAlignment="Center"
                                                              MaxWidth="400"/>
                                                </StackPanel>
                                            </Border>
                                        </WrapPanel>
                                    </Grid>
                                </ScrollViewer>
                            </Grid>
                        </ScrollViewer>
                    </TabItem>

                    $afterTab
"@

# Combine
$newContent = $beforeWrap + $cleanWrapPanel

# Save
$newContent | Out-File "MainWindow.xaml" -Encoding UTF8 -NoNewline

Write-Host "Done! File cleaned."
