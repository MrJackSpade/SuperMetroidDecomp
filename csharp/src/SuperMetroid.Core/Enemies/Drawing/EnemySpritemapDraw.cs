namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>One enemy spritemap draw: its bank, pointer, origin, palette and tile base, and clipping (#633).</summary>
internal sealed record EnemySpritemapDraw
{
    /// <summary>Bank.</summary>
    public required byte Bank { get; init; }

    /// <summary>Pointer.</summary>
    public required ushort Pointer { get; init; }

    /// <summary>Origin.</summary>
    public required Position16<OamPixels> Origin { get; init; }

    /// <summary>Palette bits.</summary>
    public required ushort PaletteBits { get; init; }

    /// <summary>Base tile index.</summary>
    public required ushort BaseTileIndex { get; init; }

    /// <summary>Clip vertical wrap.</summary>
    public bool ClipVerticalWrap { get; init; } = false;

    /// <summary>Origin y is on screen.</summary>
    public bool OriginYIsOnScreen { get; init; } = true;
}
