namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Cartridge values and helpers of <see cref="ChozoAndTubeColorRomData"/> that only verification reads.</summary>
internal static class ChozoAndTubeColorRomDataConstants
{
    /// <summary>First CGRAM color-word index, not a byte address: 144 starts OBJ palette 1, and the 32-color transfer covers indices 144 through 175 (OBJ palettes 1 and 2), matching the native target-palette rows.</summary>
    public const int Destination = 144;
}
