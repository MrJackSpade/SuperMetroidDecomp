using System.Collections.Frozen;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The logo atlas tile each piece of the right wrap is drawn from.</summary>
internal static class EndingLogoWrapTileDefinitions
{
    /// <summary>The atlas tile of each packed edge, cap, span and patch piece.</summary>
    internal static FrozenDictionary<EndingLogoWrapTile, int> Tiles { get; } = new Dictionary<EndingLogoWrapTile, int>
    {
        [EndingLogoWrapTile.InnerSideTop] = 0x00,
        [EndingLogoWrapTile.ClosingRightSpan] = 0x09,
        [EndingLogoWrapTile.ClosingMiddleSpan] = 0x0b,
        [EndingLogoWrapTile.ClosingLeftSpan] = 0x0d,
        [EndingLogoWrapTile.ClosingOuterLeftTip] = 0x0f,
        [EndingLogoWrapTile.InnerSideBottom] = 0x10,
        [EndingLogoWrapTile.ClosingRightCorner] = 0x29,
        [EndingLogoWrapTile.ClosingInnerLeftTip] = 0x2b,
        [EndingLogoWrapTile.ClosingInnerRightTip] = 0x2c,
        [EndingLogoWrapTile.LeftCap] = 0x48,
        [EndingLogoWrapTile.RightCap] = 0x49,
        [EndingLogoWrapTile.UpperSpanLeft] = 0x4c,
        [EndingLogoWrapTile.UpperSpanRight] = 0x4e,
        [EndingLogoWrapTile.PatchTopLeft] = 0x68,
        [EndingLogoWrapTile.PatchTopRight] = 0x69,
        [EndingLogoWrapTile.InnerEdgeLeft] = 0x6a,
        [EndingLogoWrapTile.InnerEdgeRight] = 0x6b,
        [EndingLogoWrapTile.PatchBottomLeft] = 0x78,
        [EndingLogoWrapTile.PatchBottomRight] = 0x79,
    }.ToFrozenDictionary();

    /// <summary>The atlas tile of each separately packed corner and side endpoint.</summary>
    internal static FrozenDictionary<EndingLogoWrapCorner, int> Corners { get; } = new Dictionary<EndingLogoWrapCorner, int>
    {
        [EndingLogoWrapCorner.UpperSideEndpoint] = 0x01,
        [EndingLogoWrapCorner.LowerSideEndpoint] = 0x20,
        [EndingLogoWrapCorner.LeftCorner] = 0x5b,
        [EndingLogoWrapCorner.InnerSideJunction] = 0x6c,
        [EndingLogoWrapCorner.OuterSideJunction] = 0x6d,
    }.ToFrozenDictionary();

    /// <summary>The edge, cap, span or patch piece each of those atlas tiles holds.</summary>
    internal static FrozenDictionary<int, EndingLogoWrapTile> TilePieces { get; } =
        Tiles.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The corner or endpoint each of those atlas tiles holds.</summary>
    internal static FrozenDictionary<int, EndingLogoWrapCorner> CornerPieces { get; } =
        Corners.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);
}
