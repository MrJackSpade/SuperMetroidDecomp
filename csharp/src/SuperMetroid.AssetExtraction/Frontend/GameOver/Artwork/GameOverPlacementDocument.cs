using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>The installed game-over layout document (#1638): the Baby's and the cursor's anchors and the cursor's frame duration.</summary>
public sealed record GameOverPlacementDocument
{
    /// <summary>Layout revision; must equal <see cref="GameOverLayoutFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Shared Baby and egg drawing origin in screen pixels, X=0-255 and Y=0-255.</summary>
    public required Position16<SpritePixels> BabyAnchor { get; init; }
    /// <summary>Common horizontal missile-cursor position in screen pixels, 0-255.</summary>
    public required int CursorX { get; init; }
    /// <summary>Vertical missile-cursor position for the Yes answer in screen pixels, 0-255.</summary>
    public required int YesCursorY { get; init; }
    /// <summary>Vertical missile-cursor position for the No answer in screen pixels, 0-255.</summary>
    public required int NoCursorY { get; init; }
    /// <summary>Duration 1-65535 in menu updates for each looping missile-cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
