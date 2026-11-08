namespace SuperMetroid.Core.Fx;

/// <summary>Whether a room animates Samus's visor colour (#1545), as the room's FX record says.</summary>
/// <remarks>The values are arbitrary.</remarks>
public enum RoomVisorAnimation
{
    /// <summary>The visor keeps its colour.</summary>
    Steady,

    /// <summary>The visor cycles through its room shades.</summary>
    Animated,
}
