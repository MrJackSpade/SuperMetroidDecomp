namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>The two menu atlas cells one large game-over letter shows, one above the other.</summary>
/// <param name="Top">The upper half's cell.</param>
/// <param name="Bottom">The lower half's cell.</param>
internal readonly record struct GameOverLargeGlyph(int Top, int Bottom);
