using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the two Brinstar blue-spore effects and the colours each writes, for the import.</summary>
/// <remarks>
/// Both effects show the same colours; the Spore Spawn room's deletes itself once the area's mini-boss dies.
/// </remarks>
public static class BrinstarBlueSporePaletteFxColorDefinitions
{
    /// <summary>The ordinary Brinstar entry without a boss-death pre-instruction.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition Standard =
        new()
        {
            Owner = BrinstarBlueSporePaletteOwner.StandardRooms,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.BrinstarBlueBackgroundSpores, ColorsPerFrame),
        };
    /// <summary>The Spore Spawn entry that deletes itself after the area's mini-boss dies.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition SporeSpawn =
        new()
        {
            Owner = BrinstarBlueSporePaletteOwner.SporeSpawnRoom,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.SporeSpawnBlueBackgroundSpores, ColorsPerFrame),
        };

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    private const int ColorsPerFrame = 3;

    /// <summary>The standard-room and Spore Spawn variants in definition order.</summary>
    public static IReadOnlyList<BrinstarBlueSporePaletteFxColorDefinition> All { get; } = [Standard, SporeSpawn];
}
