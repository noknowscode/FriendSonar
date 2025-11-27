using System;
using Windows.UI;

namespace FriendSonar.Models;

public class Friend
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double DistanceMilesValue { get; set; } // Actual distance in miles
    public int Angle { get; set; } // 0-359 degrees from top, clockwise
    public FriendStatus Status { get; set; }
    public string Emoji { get; set; } = "👤"; // Avatar emoji
    public int? EtaMinutes { get; set; } // Estimated time of arrival in minutes

    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }

    public string DistanceMiles => $"{DistanceMilesValue:F1} MI";
    public string BearingDegrees => $"{Angle}°";
    public string DistanceAndBearing => $"{DistanceMiles}  ·  {BearingDegrees}";
    public string TooltipText => $"{Name} - {DistanceAndBearing}";

    public string StatusText => Status switch
    {
        FriendStatus.Active => "ACTIVE",
        FriendStatus.Idle => "IDLE",
        FriendStatus.Away => "AWAY",
        _ => "UNKNOWN"
    };

    public string EtaText => EtaMinutes.HasValue
        ? $"↑ ETA ~{EtaMinutes} MIN"
        : "STATIONARY";

    public Color StatusColor => Status switch
    {
        FriendStatus.Active => Color.FromArgb(0xFF, 0x00, 0xFF, 0x41), // Phosphor Green
        FriendStatus.Idle => Color.FromArgb(0xFF, 0xFF, 0xAA, 0x00),   // Amber
        FriendStatus.Away => Color.FromArgb(0xFF, 0xFF, 0x41, 0x41),   // Red
        _ => Color.FromArgb(0xFF, 0x00, 0xFF, 0x41)
    };
}

public enum FriendStatus
{
    Active,
    Idle,
    Away
}
