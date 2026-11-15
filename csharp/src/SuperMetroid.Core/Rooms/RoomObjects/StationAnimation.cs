namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The station animations (#1554, #1994), each a segment of <see cref="StationAnimationDefinitions.Segments"/>. The values
/// are arbitrary and nothing reads them; each station's layout names the animations it shows (#1414).
/// </summary>
internal enum StationAnimation
{
    /// <summary>The map station's display glowing slowly.</summary>
    MapIdle,
    /// <summary>The map station's display glowing quickly once its area map is held.</summary>
    MapAcquired,
    /// <summary>The energy station's glow.</summary>
    Energy,
    /// <summary>The missile station's glow.</summary>
    Missile,
    /// <summary>The save station's idle pod, shown once before its trigger rests.</summary>
    SaveIdle,
    /// <summary>The save pod's lights flashing on and off while the game saves.</summary>
    SaveFlash,
}
