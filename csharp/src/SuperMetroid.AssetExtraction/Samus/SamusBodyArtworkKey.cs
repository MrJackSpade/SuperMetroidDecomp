namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The top and bottom halves of a Samus body artwork.</summary>
/// <param name="Top">The top half's ordinal, or null for none.</param>
/// <param name="Bottom">The bottom half's ordinal, or null for none.</param>
public readonly record struct SamusBodyArtworkKey(int? Top, int? Bottom)
{
    /// <summary>The distinct bottom-half variants each top half spans, the absent bottom half included.</summary>
    private const int BottomVariantSpan = 65536;

    /// <summary>The sprite artwork variant that names this pair of halves.</summary>
    public int SpriteVariant => VariantOf(Top) * BottomVariantSpan + VariantOf(Bottom);

    /// <summary>A half's place among its variants: an absent half is the first, and each half the one after its ordinal.</summary>
    private static int VariantOf(int? half) => half is int ordinal ? ordinal + 1 : 0;
}
