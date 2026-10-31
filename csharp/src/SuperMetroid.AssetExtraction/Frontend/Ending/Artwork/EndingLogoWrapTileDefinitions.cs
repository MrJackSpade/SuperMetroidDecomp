using System.Collections.Frozen;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The logo atlas tile each piece of the right wrap is drawn from.</summary>
internal static class EndingLogoWrapTileDefinitions
{
    /// <summary>The atlas tile of each packed edge, cap, span and patch piece.</summary>
    internal static FrozenDictionary<EndingLogoWrapTile, int> Tiles { get; } = new Dictionary<EndingLogoWrapTile, int>
    {
        [EndingLogoWrapTile.InnerSideTop] = 0,
        [EndingLogoWrapTile.ClosingRightSpan] = 9,
        [EndingLogoWrapTile.ClosingMiddleSpan] = 11,
        [EndingLogoWrapTile.ClosingLeftSpan] = 13,
        [EndingLogoWrapTile.ClosingOuterLeftTip] = 15,
        [EndingLogoWrapTile.InnerSideBottom] = 16,
        [EndingLogoWrapTile.ClosingRightCorner] = 41,
        [EndingLogoWrapTile.ClosingInnerLeftTip] = 43,
        [EndingLogoWrapTile.ClosingInnerRightTip] = 44,
        [EndingLogoWrapTile.LeftCap] = 72,
        [EndingLogoWrapTile.RightCap] = 73,
        [EndingLogoWrapTile.UpperSpanLeft] = 76,
        [EndingLogoWrapTile.UpperSpanRight] = 78,
        [EndingLogoWrapTile.PatchTopLeft] = 104,
        [EndingLogoWrapTile.PatchTopRight] = 105,
        [EndingLogoWrapTile.InnerEdgeLeft] = 106,
        [EndingLogoWrapTile.InnerEdgeRight] = 107,
        [EndingLogoWrapTile.PatchBottomLeft] = 120,
        [EndingLogoWrapTile.PatchBottomRight] = 121,
    }.ToFrozenDictionary();

    /// <summary>The atlas tile of each separately packed corner and side endpoint.</summary>
    internal static FrozenDictionary<EndingLogoWrapCorner, int> Corners { get; } = new Dictionary<EndingLogoWrapCorner, int>
    {
        [EndingLogoWrapCorner.UpperSideEndpoint] = 1,
        [EndingLogoWrapCorner.LowerSideEndpoint] = 32,
        [EndingLogoWrapCorner.LeftCorner] = 91,
        [EndingLogoWrapCorner.InnerSideJunction] = 108,
        [EndingLogoWrapCorner.OuterSideJunction] = 109,
    }.ToFrozenDictionary();

    /// <summary>The edge, cap, span or patch piece each of those atlas tiles holds.</summary>
    internal static FrozenDictionary<int, EndingLogoWrapTile> TilePieces { get; } =
        Tiles.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The corner or endpoint each of those atlas tiles holds.</summary>
    internal static FrozenDictionary<int, EndingLogoWrapCorner> CornerPieces { get; } =
        Corners.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);
}
