namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Crateria's paired late-escape palette loops.</summary>
/// <remarks>
/// Definitions <c>$FFE9</c> and <c>$FFED</c> share an eleven-record duration schedule.
/// Their 176 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class CrateriaEscapeLightningPaletteFxColorDefinitions
{
    /// <summary>Both loops contain eleven timed records.</summary>
    public const int FrameCount = 11;

    /// <summary>$8D:FFE9 yellow-lightning program targets eleven colors from CGRAM byte $A2.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition YellowLightning = new(
        CrateriaEscapeLightningPaletteOwner.YellowLightning, PaletteFxObject.CrateriaEscapeYellowLightning,
        625, 0x00a2, 11);
    /// <summary>$8D:FFED CRE pixel program targets the shared five-color tail from CGRAM byte $AE.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition CreBlockPixel = new(
        CrateriaEscapeLightningPaletteOwner.CreBlockPixel, PaletteFxObject.CrateriaEscapeCreBlockPixel,
        636, 0x00ae, 5);
    /// <summary>The yellow-lightning and CRE-pixel programs in definition order.</summary>
    public static IReadOnlyList<CrateriaEscapeLightningPaletteFxColorDefinition> All { get; } = [YellowLightning, CreBlockPixel];

}
