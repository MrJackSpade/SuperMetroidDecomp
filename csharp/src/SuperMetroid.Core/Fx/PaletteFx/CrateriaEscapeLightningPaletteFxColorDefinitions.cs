namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Crateria's paired late-escape palette loops.</summary>
/// <remarks>
/// Definitions <c>$FFE9</c> and <c>$FFED</c> share an eleven-record duration schedule.
/// Their 176 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class CrateriaEscapeLightningPaletteFxColorDefinitions
{

    /// <summary>$8D:FFE9 yellow-lightning program targets eleven colors from CGRAM byte $A2.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition YellowLightning = new()
    {
        Owner = CrateriaEscapeLightningPaletteOwner.YellowLightning,
        Object = PaletteFxObject.CrateriaEscapeYellowLightning,
        ColorsPerFrame = 11,
        Rows = PaletteFxColorRows.Of(PaletteFxProgram.Crateria41, 11),
    };
    /// <summary>$8D:FFED CRE pixel program targets the shared five-color tail from CGRAM byte $AE.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition CreBlockPixel = new()
    {
        Owner = CrateriaEscapeLightningPaletteOwner.CreBlockPixel,
        Object = PaletteFxObject.CrateriaEscapeCreBlockPixel,
        ColorsPerFrame = 5,
        Rows = PaletteFxColorRows.Of(PaletteFxProgram.Crateria401, 5),
    };
    /// <summary>The yellow-lightning and CRE-pixel programs in definition order.</summary>
    public static IReadOnlyList<CrateriaEscapeLightningPaletteFxColorDefinition> All { get; } = [YellowLightning, CreBlockPixel];

}
