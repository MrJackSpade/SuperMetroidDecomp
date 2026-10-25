namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect a room-FX record selects. Its values are arbitrary; the import decodes the record's dispatcher
/// byte (#1415).
/// </summary>
public enum RoomFxType : ushort
{
    /// <summary>No animated bank-$83 room-FX object is present.</summary>
    None = 0,
    /// <summary>Animated damaging lava surface with BG3 tiles, palette effects, and optional BG2 distortion.</summary>
    Lava = 1,
    /// <summary>Animated damaging acid surface sharing the liquid renderer but using acid-specific gameplay damage.</summary>
    Acid,
    /// <summary>Animated water surface with submerged physics and optional per-scanline BG2 displacement.</summary>
    Water,
    /// <summary>Full-screen drifting spore atmosphere driven through the room-FX BG3 owner.</summary>
    Spores,
    /// <summary>Full-screen rain atmosphere whose BG3 plane is additively composited.</summary>
    Rain,
    /// <summary>Full-screen fog atmosphere with its dedicated additive screen arrangement.</summary>
    Fog,
    /// <summary>Scrolling exterior-sky background driven by the active room's sky tilemap.</summary>
    ScrollingSky,
    /// <summary>Unused retail dispatcher entry <c>$22</c> that targets the alternate scrolling-sky routine.</summary>
    UnusedScrollingSky,
    /// <summary>Fireflea-room darkness and palette behavior, including enemy-driven light flashes.</summary>
    Fireflea,
    /// <summary>Tourian entrance statue liquid effect, rendered through the water-compatible BG3 path.</summary>
    TourianEntranceStatue,
    /// <summary>Ceres Ridley encounter effect that coordinates the room's scripted Mode 7 presentation.</summary>
    CeresRidley,
    /// <summary>Ceres elevator-shaft effect used during the station's scripted descent and destruction flow.</summary>
    CeresElevator,
    /// <summary>Ceres destruction haze whose palette and layer state progress with the escape sequence.</summary>
    CeresHaze,
}
