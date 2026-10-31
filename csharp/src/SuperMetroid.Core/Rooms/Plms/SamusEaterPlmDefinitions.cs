namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The two Samus Eater block actors, the Brinstar floor and ceiling plants (block actors, not Yapping Maws): the floor
/// plant takes the floor setup and bite, the ceiling plant the ceiling setup and bite. Mounting direction follows the
/// setup, including opposite alignment edges and held-Y adjustments.
/// </summary>
internal static class SamusEaterPlmDefinitions
{
    /// <summary>The Brinstar floor plant and its first program.</summary>
    public static readonly SamusEaterPlmDefinition Floor =
        new(SamusEaterProgram.FloorBite, Ceiling: false);

    /// <summary>The Brinstar ceiling plant and its first program.</summary>
    public static readonly SamusEaterPlmDefinition Ceiling =
        new(SamusEaterProgram.CeilingBite, Ceiling: true);

    /// <summary>The chew sound.</summary>
    public static readonly SoundEffectId ChewSound = Library2Sound.PlantChewing;

    /// <summary>The invincibility bit the hold ORs into Samus's invincibility timer each frame.</summary>
    public const int HeldInvincibilityBit = 1 << 4;

    /// <summary>The immunity the release gives Samus, in frames.</summary>
    public const int ReleaseInvincibilityFrames = 48;

    /// <summary>The periodic damage each chew deals.</summary>
    public const int ChewDamage = 2;

    /// <summary>The plant each supported header places; every other header is not a Samus Eater.</summary>
    public static IReadOnlyDictionary<PlmHeaderId, SamusEaterPlmDefinition> ByHeader { get; } =
        new Dictionary<PlmHeaderId, SamusEaterPlmDefinition>
        {
            [PlmHeaderId.InsideReactionBrinstarFloorPlant] = Floor,
            [PlmHeaderId.InsideReactionBrinstarCeilingPlant] = Ceiling,
        };
}
