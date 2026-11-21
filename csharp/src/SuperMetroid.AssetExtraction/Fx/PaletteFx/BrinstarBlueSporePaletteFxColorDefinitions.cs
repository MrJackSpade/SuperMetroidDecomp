using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the two Brinstar blue-spore effects and the colours each writes, for the import.</summary>
/// <remarks>
/// Both effects show the same colours; the Spore Spawn room's deletes itself once the area's mini-boss dies.
/// </remarks>
public static partial class BrinstarBlueSporePaletteFxColorDefinitions
{
    /// <summary>The ordinary Brinstar entry without a boss-death pre-instruction.</summary>
    internal static readonly BrinstarBlueSporePaletteFxColorDefinition BrinstarBlueBackgroundSpores =
        new(ColourEffectId.BrinstarBlueBackgroundSpores, ColorsPerFrame);
    /// <summary>The Spore Spawn entry that deletes itself after the area's mini-boss dies.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition SporeSpawnBlueBackgroundSpores =
        new(ColourEffectId.SporeSpawnBlueBackgroundSpores, ColorsPerFrame);

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    internal const int ColorsPerFrame = 3;

    /// <summary>The standard-room and Spore Spawn variants in definition order.</summary>
    public static IReadOnlyList<BrinstarBlueSporePaletteFxColorDefinition> All { get; } = [BrinstarBlueBackgroundSpores, SporeSpawnBlueBackgroundSpores];
}
