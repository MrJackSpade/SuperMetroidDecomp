namespace SuperMetroid.Core.Fx;

/// <summary>
/// The kind of room effect a room shows: a liquid, an atmosphere, a sky, a darkness or a scripted scene's effect. Its
/// values are arbitrary (#1415, #1740).
/// </summary>
public enum RoomFxType
{
    /// <summary>The room shows no effect of its own.</summary>
    None,
    /// <summary>A lava surface that burns Samus, with its overlay pictures and glows, and in some rooms a rippling background.</summary>
    Lava,
    /// <summary>An acid surface, shown as lava is, that deals acid's own damage.</summary>
    Acid,
    /// <summary>A water surface below which Samus moves as submerged, in some rooms with the background waving beneath it.</summary>
    Water,
    /// <summary>Spores drifting over the whole screen.</summary>
    Spores,
    /// <summary>Rain falling over the whole screen, brightening what it covers.</summary>
    Rain,
    /// <summary>Fog over the whole screen, with its own fog picture brightening what it covers.</summary>
    Fog,
    /// <summary>The sky outside, scrolling behind the room in the bands of the room's sky picture.</summary>
    ScrollingSky,
    /// <summary>The Fireflea rooms' darkness, which flashes in a cycle and deepens with each Fireflea killed.</summary>
    Fireflea,
    /// <summary>The surface of the statue room before Tourian, which Samus moves below as she does under water.</summary>
    TourianEntranceStatue,
    /// <summary>Ridley's room on Ceres, whose turning floor the encounter's scenes move.</summary>
    CeresRidley,
    /// <summary>The Ceres elevator shaft, as Samus descends it and as the station is destroyed.</summary>
    CeresElevator,
    /// <summary>The haze of Ceres's destruction, whose colours strengthen as the escape runs.</summary>
    CeresHaze,
}
