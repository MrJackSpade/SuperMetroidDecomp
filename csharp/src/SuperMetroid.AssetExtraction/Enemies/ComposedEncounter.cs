namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// An encounter whose artwork the import composes in looks (#1538): the authored looks it shows as it plays out, each
/// what it copies over the characters its room's load leaves. Gameplay shows the looks of the encounters it names
/// (<see cref="SuperMetroid.Core.Assets.EncounterArtwork"/>); every frame of the others is drawn in one look, which is its
/// one finished picture. The values are arbitrary identities, in the order frames name the looks they show.
/// </summary>
internal enum ComposedEncounter
{
    /// <summary>Draygon's opening, which readies the Evirs she releases.</summary>
    DraygonEvirs,

    /// <summary>The gunship, whose engines fire up for its takeoff.</summary>
    Gunship,

    /// <summary>Crocomire, whose death bares his skeleton.</summary>
    Crocomire,

    /// <summary>Bomb Torizo's crumbling statue, gut and face.</summary>
    BombTorizo,

    /// <summary>Golden Torizo's awakening, egg release and eye glow.</summary>
    GoldenTorizo,

    /// <summary>The Ceres self-destruct's escape timer and warning.</summary>
    CeresEscape,

    /// <summary>A Ceres door's pre-explosion overlay.</summary>
    CeresDoor,

    /// <summary>Mother Brain's legs, the Baby, her attacks, her corpse and the escape.</summary>
    MotherBrain,
}
