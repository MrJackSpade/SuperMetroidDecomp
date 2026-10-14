namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verified even room-FX dispatcher values from the bank-$83 function table. Values $0E
/// through $1E share a no-op RTL and remain unnamed at the port validation boundary.
/// </summary>
public enum RoomFxType : ushort
{
    /// <summary>No animated bank-$83 room-FX object is present.</summary>
    None = 0x0,
    /// <summary>Animated damaging lava surface with BG3 tiles, palette effects, and optional BG2 distortion.</summary>
    Lava = 0x2,
    /// <summary>Animated damaging acid surface sharing the liquid renderer but using acid-specific gameplay damage.</summary>
    Acid = 0x4,
    /// <summary>Animated water surface with submerged physics and optional per-scanline BG2 displacement.</summary>
    Water = 0x6,
    /// <summary>Full-screen drifting spore atmosphere driven through the room-FX BG3 owner.</summary>
    Spores = 0x8,
    /// <summary>Full-screen rain atmosphere whose BG3 plane is additively composited.</summary>
    Rain = 0x0a,
    /// <summary>Full-screen fog atmosphere with its dedicated additive screen arrangement.</summary>
    Fog = 0x0c,
    /// <summary>Scrolling exterior-sky background driven by the active room's sky tilemap.</summary>
    ScrollingSky = 0x20,
    /// <summary>Unused retail dispatcher entry <c>$22</c> that targets the alternate scrolling-sky routine.</summary>
    UnusedScrollingSky = 0x22,
    /// <summary>Fireflea-room darkness and palette behavior, including enemy-driven light flashes.</summary>
    Fireflea = 0x24,
    /// <summary>Tourian entrance statue liquid effect, rendered through the water-compatible BG3 path.</summary>
    TourianEntranceStatue = 0x26,
    /// <summary>Ceres Ridley encounter effect that coordinates the room's scripted Mode 7 presentation.</summary>
    CeresRidley = 0x28,
    /// <summary>Ceres elevator-shaft effect used during the station's scripted descent and destruction flow.</summary>
    CeresElevator = 0x2a,
    /// <summary>Ceres destruction haze whose palette and layer state progress with the escape sequence.</summary>
    CeresHaze = 0x2c,
}
