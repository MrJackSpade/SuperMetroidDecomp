namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="ChainsawBeamGraphicsDefinitions"/> that only verification reads.</summary>
internal static class ChainsawBeamGraphicsDefinitionsConstants
{
    /// <summary>$90:8000..801E, the Samus_Animate instruction prefix read after the first open-bus byte.</summary>
    internal const int InstructionAddress = 0x908000;

    /// <summary>
    /// The byte <c>LDA [$00],Y</c> reads at <c>$90:7FFF</c>: the expansion window is open bus, so it returns the memory
    /// data register, which still holds the long pointer's bank, $90.
    /// </summary>
    internal const byte OpenBusByte = 0x90;
    /// <summary>$9A:C421: $90:C3CB (the second palette-pointer word) reinterpreted by $90:AC8D as the tile pointer.</summary>
    internal const int TileSource = 0x9ac421;
}
