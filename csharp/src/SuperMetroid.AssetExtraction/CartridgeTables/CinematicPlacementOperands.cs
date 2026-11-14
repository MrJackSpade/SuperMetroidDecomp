namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>How a cinematic actor's placement initializer lays out its two coordinate immediates.</summary>
internal static class CinematicPlacementOperands
{
    /// <summary>The bytes from the first immediate to the second: the first immediate's word and the store between them.</summary>
    internal const int Distance = 6;
}
