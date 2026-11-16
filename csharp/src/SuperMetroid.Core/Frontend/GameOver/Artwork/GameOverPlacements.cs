namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Where the game-over screen places the Baby, the egg and the missile cursor and how long the cursor holds each frame:
/// the part of the installed game-over presentation the game reads (#1502).
/// </summary>
public sealed record GameOverPlacements
{
    /// <summary>Shared Baby and egg drawing origin on the screen, in pixels.</summary>
    public required Offset16<ScreenPixels> BabyAnchor { get; init; }
    /// <summary>The missile cursor's horizontal place on the screen, in pixels, shared by both answers.</summary>
    public required int CursorX { get; init; }
    /// <summary>The missile cursor's vertical place on the screen for the Yes answer, in pixels.</summary>
    public required int YesCursorY { get; init; }
    /// <summary>The missile cursor's vertical place on the screen for the No answer, in pixels.</summary>
    public required int NoCursorY { get; init; }
    /// <summary>Duration in menu updates, at least one, of each looping missile-cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
