namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable control words for the Bomb and Golden Torizo belly palette loops.</summary>
/// <remarks>
/// Palette-FX definitions <c>$F759</c> and <c>$F75D</c> install matching six-frame
/// programs with different live BGR555 colors. This catalog owns only color-index setup,
/// enemy-death pre-instruction setup, durations, waits, and loop control.
/// Bomb and Golden programs start at $8D:E2E9 and $8D:E331, each selecting
/// CGRAM byte $0132 and installing enemy-zero-death pre-instruction $E2E0.
/// Records f=0..5 begin at $E2F1 or $E339 plus 10*f, write three live
/// colors, and end in $C595 wait. Gotos at $E32D and $E375 return to
/// the respective first records after a 52-frame cycle; f=6 reaches
/// control. All 36 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class TorizoBellyPaletteFxColorDefinitions
{

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The Bomb and Golden Torizo programs in palette-definition order.</summary>
    public static IReadOnlyList<TorizoBellyPaletteFxColorDefinition> All { get; } =
    [
        new(TorizoBellyPaletteOwner.BombTorizo, PaletteFxObject.BombTorizoBelly, PaletteFxProgram.BombTorizoBelly1),
        new(TorizoBellyPaletteOwner.GoldenTorizo, PaletteFxObject.GoldenTorizoBelly, PaletteFxProgram.GoldenTorizoBelly1),
    ];
}
