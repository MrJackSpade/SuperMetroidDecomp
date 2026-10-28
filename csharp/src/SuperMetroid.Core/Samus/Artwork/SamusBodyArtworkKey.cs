namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The top and bottom halves of a Samus body artwork.</summary>
/// <param name="Top">The top.</param>
/// <param name="Bottom">The bottom.</param>
public readonly record struct SamusBodyArtworkKey(int Top, int Bottom)
{
    /// <summary>The distinct bottom-half variants each top half spans, the absent bottom half included.</summary>
    private const int BottomVariantSpan = 65536;

    /// <summary>The sprite artwork variant that names this pair of halves; an absent half (-1) contributes zero.</summary>
    public int SpriteVariant => (Top + 1) * BottomVariantSpan + Bottom + 1;
}
