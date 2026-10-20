using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// One enemy spritemap draw as the cartridge names it, by bank and spritemap word: what the reference decoder reads,
/// and what a native fixture hands the production renderer through its frame view.
/// </summary>
internal sealed record NativeEnemySpritemapDraw
{
    /// <summary>The enemy bank holding the spritemap.</summary>
    public required byte Bank { get; init; }

    /// <summary>The bank-local spritemap word.</summary>
    public required ushort Pointer { get; init; }

    /// <summary>Origin.</summary>
    public required Position16<OamPixels> Origin { get; init; }

    /// <summary>Palette bits.</summary>
    public required int PaletteBits { get; init; }

    /// <summary>Base tile index.</summary>
    public required int BaseTileIndex { get; init; }

    /// <summary>Clip vertical wrap.</summary>
    public bool ClipVerticalWrap { get; init; } = false;

    /// <summary>Origin y is on screen.</summary>
    public bool OriginYIsOnScreen { get; init; } = true;
}
