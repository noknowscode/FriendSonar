using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using FriendSonar.Models;

namespace FriendSonar;

public sealed partial class MainPage : Page
{
    private DispatcherTimer? _statusTimer;
    private TextBlock? _pingAngle;
    private TextBlock? _contactCount;
    private TextBlock? _rangeText;
    private TextBlock? _lastScanText;
    private Controls.RadarDisplay? _radarDisplay;
    private Controls.ContactList? _contactListControl;
    private Controls.FriendDetailPanel? _friendDetailPanel;

    // Range toggle buttons
    private ToggleButton? _range1Button;
    private ToggleButton? _range3Button;
    private ToggleButton? _range5Button;
    private ToggleButton? _range10Button;

    // Last scan tracking
    private DateTime _lastScanTime;
    private int _currentRange = 3;

    public ObservableCollection<Friend> Friends { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
        this.Unloaded += MainPage_Unloaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Get references to named elements
        _pingAngle = this.FindName("PingAngle") as TextBlock;
        _contactCount = this.FindName("ContactCount") as TextBlock;
        _rangeText = this.FindName("RangeText") as TextBlock;
        _lastScanText = this.FindName("LastScanText") as TextBlock;
        _radarDisplay = this.FindName("RadarDisplay") as Controls.RadarDisplay;
        _contactListControl = this.FindName("ContactListControl") as Controls.ContactList;
        _friendDetailPanel = this.FindName("FriendDetailPanel") as Controls.FriendDetailPanel;

        // Wire up blip tap event
        if (_radarDisplay != null)
        {
            _radarDisplay.BlipTapped += RadarDisplay_BlipTapped;
        }

        // Get range button references
        _range1Button = this.FindName("Range1Button") as ToggleButton;
        _range3Button = this.FindName("Range3Button") as ToggleButton;
        _range5Button = this.FindName("Range5Button") as ToggleButton;
        _range10Button = this.FindName("Range10Button") as ToggleButton;

        // Initialize last scan time
        _lastScanTime = DateTime.Now;

        InitializeSampleData();
        StartStatusTimer();
    }

    private void RangeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton clickedButton) return;

        // Uncheck all buttons except the clicked one
        if (_range1Button != null) _range1Button.IsChecked = _range1Button == clickedButton;
        if (_range3Button != null) _range3Button.IsChecked = _range3Button == clickedButton;
        if (_range5Button != null) _range5Button.IsChecked = _range5Button == clickedButton;
        if (_range10Button != null) _range10Button.IsChecked = _range10Button == clickedButton;

        // Parse the range from the Tag property
        if (clickedButton.Tag is string tagStr && int.TryParse(tagStr, out var range))
        {
            _currentRange = range;
            _radarDisplay?.SetRange(range);
            UpdateRangeText();
        }
    }

    private void UpdateRangeText()
    {
        if (_rangeText != null)
        {
            _rangeText.Text = $"RANGE: {_currentRange} MI";
        }
    }

    private void RadarDisplay_BlipTapped(object? sender, Friend friend)
    {
        _friendDetailPanel?.ShowFriend(friend);
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        StopStatusTimer();
    }

    private void InitializeSampleData()
    {
        if (_radarDisplay == null || _contactListControl == null) return;

        // Add sample friends at various distances to test range filtering
        var sampleFriends = new[]
        {
            // Very close (< 1 mile)
            new Friend { Id = 1, Name = "Sarah", DistanceMilesValue = 0.5, Angle = 45, Status = FriendStatus.Active, Emoji = "👩", EtaMinutes = 3 },
            new Friend { Id = 2, Name = "Dev", DistanceMilesValue = 0.8, Angle = 310, Status = FriendStatus.Active, Emoji = "👨‍💻", EtaMinutes = 5 },
            // Medium distance (1-3 miles)
            new Friend { Id = 3, Name = "Marcus", DistanceMilesValue = 1.5, Angle = 120, Status = FriendStatus.Active, Emoji = "👨", EtaMinutes = 8 },
            new Friend { Id = 4, Name = "Luna", DistanceMilesValue = 2.3, Angle = 170, Status = FriendStatus.Away, Emoji = "🌙", EtaMinutes = null },
            // Further (3-5 miles)
            new Friend { Id = 5, Name = "Jen", DistanceMilesValue = 4.2, Angle = 230, Status = FriendStatus.Idle, Emoji = "👩‍🎨", EtaMinutes = null },
            new Friend { Id = 6, Name = "Alex", DistanceMilesValue = 3.8, Angle = 85, Status = FriendStatus.Active, Emoji = "🧑", EtaMinutes = 15 },
            // Far (5-10 miles)
            new Friend { Id = 7, Name = "Chris", DistanceMilesValue = 6.5, Angle = 200, Status = FriendStatus.Active, Emoji = "👨‍🦱", EtaMinutes = 20 },
            new Friend { Id = 8, Name = "Taylor", DistanceMilesValue = 8.7, Angle = 340, Status = FriendStatus.Idle, Emoji = "🧑‍🎤", EtaMinutes = null }
        };

        foreach (var friend in sampleFriends)
        {
            Friends.Add(friend);
            _radarDisplay.AddFriend(friend.Id, friend.Name, friend.DistanceMilesValue, friend.Angle, friend.Status);
        }

        _contactListControl.SetFriends(Friends);
        if (_contactCount != null)
        {
            _contactCount.Text = Friends.Count.ToString();
        }
    }

    private void StartStatusTimer()
    {
        _statusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _statusTimer.Tick += StatusTimer_Tick;
        _statusTimer.Start();
    }

    private void StopStatusTimer()
    {
        if (_statusTimer != null)
        {
            _statusTimer.Stop();
            _statusTimer.Tick -= StatusTimer_Tick;
            _statusTimer = null;
        }
    }

    private void StatusTimer_Tick(object? sender, object e)
    {
        // Update ping angle display
        if (_radarDisplay != null && _pingAngle != null)
        {
            var angle = _radarDisplay.CurrentSweepAngle;
            _pingAngle.Text = $"PING: {angle}°";
        }

        // Update last scan time display
        if (_lastScanText != null)
        {
            var elapsed = DateTime.Now - _lastScanTime;
            _lastScanText.Text = $"LAST SCAN: {(int)elapsed.TotalSeconds}s";
        }
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        // Get deferral to keep refresh indicator visible during async operation
        var deferral = args.GetDeferral();

        try
        {
            // Trigger full scan animation on radar
            if (_radarDisplay != null)
            {
                await _radarDisplay.TriggerFullScanAsync();
            }

            // Reset last scan time
            _lastScanTime = DateTime.Now;
        }
        finally
        {
            // Complete the refresh
            deferral.Complete();
        }
    }

    private async void PingAllButton_Click(object sender, RoutedEventArgs e)
    {
        // Trigger full scan animation on radar (pings all visible friends)
        if (_radarDisplay != null)
        {
            await _radarDisplay.TriggerFullScanAsync();
        }

        // Reset last scan time
        _lastScanTime = DateTime.Now;
    }
}
