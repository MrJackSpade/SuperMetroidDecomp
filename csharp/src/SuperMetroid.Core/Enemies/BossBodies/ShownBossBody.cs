using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body as a gameplay frame shows it (#1466, #1514, #1515): pre-coloured pictures placed where they show on the
/// body's area, which repeats beyond its edges; the view places the area on screen. The pieces never overlap, and a pixel
/// no piece covers draws nothing (#1513).
/// </summary>
/// <param name="Pieces">The pieces showing.</param>
/// <param name="Area">The area the body covers before it repeats.</param>
public sealed record ShownBossBody(ImmutableArray<ShownBossBodyPiece> Pieces, Extent16<BackgroundPixels> Area)
{
    /// <summary>The area a body covers where its room shows two screens of it side by side (#1514).</summary>
    public static Extent16<BackgroundPixels> SideBySideArea { get; } = new(2 * ScreenPixels, ScreenPixels);

    /// <summary>The area a body covers where its room shows one screen of it, repeated (#1514).</summary>
    public static Extent16<BackgroundPixels> OneScreenArea { get; } = new(ScreenPixels, ScreenPixels);

    /// <summary>Pixels across and down one screen of a boss body.</summary>
    private const int ScreenPixels = 256;
}
