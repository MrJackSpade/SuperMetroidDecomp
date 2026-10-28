using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>One enemy spritemap draw: its frame, origin, palette and tile base, and clipping (#633).</summary>
internal sealed record EnemySpritemapDraw
{
    /// <summary>The installed frame drawn.</summary>
    public required EnemyFrameId Frame { get; init; }

    /// <summary>Origin.</summary>
    public required Position16<SpritePixels> Origin { get; init; }

    /// <summary>Palette bits.</summary>
    public required int PaletteBits
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Base tile index.</summary>
    public required int BaseTileIndex
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

}
