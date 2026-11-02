namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Where the game-over screen places the Baby, the egg and the missile cursor and how long the cursor holds each frame:
/// the part of the installed game-over presentation the game reads (#1502).
/// </summary>
public sealed record GameOverPlacements
{
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
