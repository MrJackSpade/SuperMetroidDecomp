namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Where verification expects the n00b tube and Chozo statue colour transfers to land.</summary>
internal static class ChozoAndTubeColorTransfer
{
    /// <summary>First CGRAM color-word index, not a byte address: 144 starts OBJ palette 1, and the 32-color transfer covers indices 144 through 175 (OBJ palettes 1 and 2), matching the native target-palette rows.</summary>
    public const int Destination = 144;
}
