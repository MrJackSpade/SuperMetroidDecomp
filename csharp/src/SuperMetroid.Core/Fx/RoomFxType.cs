namespace SuperMetroid.Core.Fx;

/// <summary>
/// The kind of room effect a room shows: a liquid, an atmosphere, a sky, a darkness or a scripted scene's effect. Its
/// values are arbitrary (#1415, #1740).
/// </summary>
public enum RoomFxType
{
    /// <summary>The room shows no effect of its own.</summary>
    None = 0,
    /// <summary>Animated damaging lava surface with the effect overlay's pictures, palette effects, and optional background distortion.</summary>
    Lava = 1,
    /// <summary>Animated damaging acid surface sharing the liquid renderer but using acid-specific gameplay damage.</summary>
    Acid,
    /// <summary>Animated water surface with submerged physics and optional per-row background displacement.</summary>
    Water,
    /// <summary>Full-screen drifting spore atmosphere driven through the room-FX effect overlay owner.</summary>
    Spores,
    /// <summary>Full-screen rain atmosphere whose effect overlay plane is additively composited.</summary>
    Rain,
    /// <summary>Full-screen fog atmosphere with its dedicated additive screen arrangement.</summary>
    Fog,
    /// <summary>Scrolling exterior-sky background driven by the active room's sky picture.</summary>
    ScrollingSky,
    /// <summary>Fireflea-room darkness and palette behavior, including enemy-driven light flashes.</summary>
    Fireflea,
    /// <summary>Tourian entrance statue liquid effect, rendered through the water-compatible effect overlay path.</summary>
    TourianEntranceStatue,
    /// <summary>Ceres Ridley encounter effect that coordinates the room's scripted turning-floor presentation.</summary>
    CeresRidley,
    /// <summary>Ceres elevator-shaft effect used during the station's scripted descent and destruction flow.</summary>
    CeresElevator,
    /// <summary>Ceres destruction haze whose palette and layer state progress with the escape sequence.</summary>
    CeresHaze,
}
