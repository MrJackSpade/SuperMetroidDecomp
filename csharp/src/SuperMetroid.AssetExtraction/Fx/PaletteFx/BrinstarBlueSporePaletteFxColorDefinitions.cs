using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable control words for the two Brinstar blue-spore palette loops.</summary>
/// <remarks>
/// The two palette-FX definitions contain the same presentation BGR555
/// sequence. The Spore Spawn variant additionally installs the area-mini-boss death
/// frame behaviour. This catalog owns setup, timing, waits, and loop control only.
/// Both select the same colour index.
/// Each valid i=0..13 lasts ten frames, writes three presentation colors, and
/// ends in a wait. Their gotos return to
/// the respective first records, giving 140-frame cycles; i=14 reaches
/// loop control. All 66 control words across both programs match the
/// original programs.
/// </remarks>
public static class BrinstarBlueSporePaletteFxColorDefinitions
{
    /// <summary>The ordinary Brinstar entry without a boss-death pre-instruction.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition Standard =
        new()
        {
            Owner = BrinstarBlueSporePaletteOwner.StandardRooms,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.BrinstarBlueBackgroundSpores, ColorsPerFrame),
        };
    /// <summary>The Spore Spawn entry that deletes itself after the area's mini-boss dies.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition SporeSpawn =
        new()
        {
            Owner = BrinstarBlueSporePaletteOwner.SporeSpawnRoom,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.SporeSpawnBlueBackgroundSpores, ColorsPerFrame),
        };

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The standard-room and Spore Spawn variants in definition order.</summary>
    public static IReadOnlyList<BrinstarBlueSporePaletteFxColorDefinition> All { get; } = [Standard, SporeSpawn];
}
