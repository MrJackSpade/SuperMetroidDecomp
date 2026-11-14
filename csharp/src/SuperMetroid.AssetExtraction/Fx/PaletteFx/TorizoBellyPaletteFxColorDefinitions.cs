using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable control words for the Bomb and Golden Torizo belly palette loops.</summary>
/// <remarks>
/// The two palette-FX definitions install matching six-frame
/// programs with different live BGR555 colors. This catalog owns only color-index setup,
/// enemy-death frame behaviour setup, durations, waits, and loop control.
/// The Bomb and Golden programs select the same colour index
/// and install the enemy-zero-death frame behaviour.
/// Records f=0..5 are ten bytes apart, write three live
/// colors, and end in a wait. Their gotos return to
/// the respective first records after a 52-frame cycle; f=6 reaches
/// control. All 36 mechanics words match the original programs.
/// </remarks>
public static class TorizoBellyPaletteFxColorDefinitions
{

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The Bomb and Golden Torizo programs in palette-definition order.</summary>
    public static IReadOnlyList<TorizoBellyPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = TorizoBellyPaletteOwner.BombTorizo,
            Object = ColourEffectId.BombTorizoBelly,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.BombTorizoBelly, ColorsPerFrame),
        },
        new()
        {
            Owner = TorizoBellyPaletteOwner.GoldenTorizo,
            Object = ColourEffectId.GoldenTorizoBelly,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.GoldenTorizoBelly, ColorsPerFrame),
        },
    ];
}
