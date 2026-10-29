namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the Zebes explosion's afterglow and lava loops.</summary>
/// <remarks>
/// Issue #846 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E1D4</c>
/// enters at <c>$8D:D3CA</c>: <c>SetColorIndex($01C2)</c>, six 20-byte
/// records of <c>16, colors[8], Wait</c>, then <c>Goto($D3CE)</c> at
/// <c>$D446</c> (96 frames per loop). All 16 control words match. Its color
/// rows are exactly <c>A,B,C,D,C,B</c>: all 16 words in rows 4 and 5 repeat
/// rows 2 and 1. Rows <c>A</c> through <c>D</c> remain authored data;
/// straight component interpolation from <c>A</c> to <c>D</c> misses 10 of
/// the 16 middle words with floor rounding and 12 with nearest rounding.
/// The presentation compiler supplies all 48 live colors while this catalog
/// supplies only controls to the palette-FX runtime.
/// Issue #847 / #625: ROM definition <c>$8D:E1D8</c> enters at
/// <c>$8D:D44A</c>: <c>SetColorIndex($0080)</c>, ten six-byte records of
/// <c>duration, color, Wait</c>, then <c>Goto($D44E)</c> at <c>$D48A</c>.
/// For record <c>f</c> (0..9), duration is <c>9 - f</c> for 0..4 and
/// <c>f</c> for 5..9 (70 frames per cycle). Its single red-only BGR555
/// color is <c>18 - 3*f</c> for 0..5 and <c>3*(f - 4)</c> for 6..9.
/// All 24 control words and ten colors match the ROM. The presentation
/// compiler still supplies the live colors. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionAmbientPaletteFxColorDefinitions
{
    /// <summary>Fixed afterglow and lava program descriptors in native definition order.</summary>
    private static readonly ZebesExplosionAmbientPaletteFxColorDefinition[] Definitions =
    [
        new()
        {
            Owner = ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow,
            Object = PaletteFxObject.ZebesExplosionPlanetAfterglow,
            ColorsPerFrame = 8,
            CycleFrames = 96,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.ZebesExplosionPlanetAfterGlow1, 8),
        },
        new()
        {
            Owner = ZebesExplosionAmbientPaletteFxProgramOwner.Lava,
            Object = PaletteFxObject.ExplodingZebesLava,
            ColorsPerFrame = 1,
            CycleFrames = 70,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.ExplodingZebesLava1, 1),
        },
    ];

    /// <summary>The afterglow and lava programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionAmbientPaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
