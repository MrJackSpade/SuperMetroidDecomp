namespace SuperMetroid.Core.Fx;

/// <summary>Whether a room cycles the shade of Samus's visor (#1545), as the room's effects say.</summary>
/// <remarks>The values are arbitrary.</remarks>
public enum RoomVisorAnimation
{
    /// <summary>The visor keeps its colour.</summary>
    Steady,

    /// <summary>The visor cycles through its room shades.</summary>
    Animated,
}
