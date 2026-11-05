namespace SuperMetroid.Core.Assets;

/// <summary>
/// An encounter whose frames change artwork as it plays out after its room loads (#1513): Draygon's opening readies the
/// Evirs she releases, the gunship's takeoff fires its engines, Crocomire's death bares his skeleton, a Torizo's statue
/// crumbles and its eyes and gut glow, Ceres Ridley's getaway brings up the escape timer and warning, a Ceres door shows its
/// overlay, Mother Brain's fight shows her legs, the Baby, her attacks, her corpse and the escape, and her corpse rots.
/// Each is a fixed sequence of steps; the encounter names the step it has reached, and every frame it changes is a
/// pre-assembled picture of that step, which the import composes. The values are arbitrary identities.
/// </summary>
public enum EncounterArtworkSequence
{
    /// <summary>Draygon's opening readying the Evirs she releases.</summary>
    DraygonEvirs,

    /// <summary>The gunship's takeoff, page by page as its engines fire up.</summary>
    GunshipLiftoff,

    /// <summary>Crocomire's death baring his skeleton, chunk by chunk.</summary>
    CrocomireSkeleton,

    /// <summary>Bomb Torizo's statue crumbling, then its gut and its face blowing up.</summary>
    BombTorizo,

    /// <summary>Golden Torizo's awakening and crumbling, its egg release and its eye beam's glow.</summary>
    GoldenTorizo,

    /// <summary>The Ceres self-destruct's escape timer and warning, page by page.</summary>
    CeresEscapePages,

    /// <summary>A Ceres door's pre-explosion overlay.</summary>
    CeresDoor,

    /// <summary>Mother Brain's legs, the Baby, her attacks, her corpse and the escape, page by page.</summary>
    MotherBrainPages,
}
