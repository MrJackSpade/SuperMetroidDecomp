namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Which sprite palette sets take part in colour math on the cartridge.</summary>
internal static class SpriteColourMathDefinitions
{
    /// <summary>The first sprite palette set whose pixels take part in colour math, as every later set's do.</summary>
    internal const int FirstSetTakingColourMath = 4;
}
