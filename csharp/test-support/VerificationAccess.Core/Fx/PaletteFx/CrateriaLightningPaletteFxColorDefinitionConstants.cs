namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="CrateriaLightningPaletteFxColorDefinition"/> that only verification reads.</summary>
internal static class CrateriaLightningPaletteFxColorDefinitionConstants
{
    /// <summary>CGRAM byte $A8, written by the live surface lightning.</summary>
    public const ushort SurfaceColorByteIndex = 0x00a8;
    /// <summary>CGRAM byte $82, written by the unused dark lightning.</summary>
    public const ushort DarkColorByteIndex = 0x0082;
}
