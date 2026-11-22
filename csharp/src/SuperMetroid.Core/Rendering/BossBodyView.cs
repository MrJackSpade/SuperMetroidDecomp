using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A boss's body as its own display layer (#1352-#1356, #2573): its parts' pictures, each behind or in front of the actors
/// at its own depth, scrolled to show where its boss places it, with the distortions its boss or the room's liquid put on its
/// lines and the pixelation its boss shows. The room's background shows nothing behind it.
/// </summary>
/// <param name="Body">The body's parts' pictures, placed where they lie against the body's placement.</param>
/// <param name="Scroll">The body pixel the screen's top-left corner shows, a signed position.</param>
public sealed partial record BossBodyView(ShownBossBody Body, Offset16<BackgroundPixels> Scroll)
{
    /// <summary>How the body's lines move sideways, line by line, or null where every line shows <see cref="Scroll"/>.</summary>
    public LineScroll? Horizontal { get; init; }

    /// <summary>How the body's lines move up and down, line by line, or null where every line shows <see cref="Scroll"/>.</summary>
    public LineScroll? Vertical { get; init; }

    /// <summary>How the body is pixelated: none, or Phantoon's blocks as he appears and fades.</summary>
    public BackgroundPixelation Pixelation { get; init; } = BackgroundPixelation.Unpixelated;
}
