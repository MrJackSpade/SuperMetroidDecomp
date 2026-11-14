namespace SuperMetroid.Core.Assets;

/// <summary>
/// An encounter whose artwork changes its authored look as it plays out after its room loads (#1513, #1538): the gunship's
/// engines fire up for its takeoff, Crocomire's death bares his skeleton, a Torizo's eyes light up and its gut, face or
/// chest changes, a Ceres door shows its overlay, and Ceres Ridley's getaway brings up the escape timer and warning. The
/// encounter names the look it shows; every frame the look changes is a finished picture of that look, which the import
/// composes. The values are arbitrary identities.
/// </summary>
public enum EncounterArtwork
{
    /// <summary>The gunship, whose engines fire up for its takeoff.</summary>
    Gunship,

    /// <summary>Crocomire, whose death bares his skeleton.</summary>
    Crocomire,

    /// <summary>Bomb Torizo: its eyes lighting up as it wakes, then its gut and its face blowing up.</summary>
    BombTorizo,

    /// <summary>Golden Torizo: its awakening and eyes lighting up, its chest's egg release and its eye beam's glow.</summary>
    GoldenTorizo,

    /// <summary>A Ceres door, whose pre-explosion overlay shows its own look.</summary>
    CeresDoor,

    /// <summary>The Ceres self-destruct, whose escape timer and warning the Ridley arena's backgrounds show.</summary>
    CeresEscape,
}
