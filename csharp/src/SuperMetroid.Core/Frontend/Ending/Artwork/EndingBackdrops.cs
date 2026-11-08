using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The solid colour each ending scene shows behind every layer once its own colours show (#1544), as the import supplies
/// it. The ending loads no colour table: every picture it draws is pre-coloured, and only these colours are its own.
/// </summary>
public sealed class EndingBackdrops
{
    /// <summary>Keeps each scene's backdrop.</summary>
    /// <param name="escape">The escape scenes' backdrop.</param>
    /// <param name="explosion">The planet explosion's and the flyaway's backdrop.</param>
    /// <param name="credits">The staff credits' backdrop.</param>
    /// <param name="postCredits">The post-credits scenes' backdrop.</param>
    public EndingBackdrops(RgbColor escape, RgbColor explosion, RgbColor credits, RgbColor postCredits)
    {
        Escape = escape;
        Explosion = explosion;
        Credits = credits;
        PostCredits = postCredits;
    }

    /// <summary>The escape scenes' backdrop.</summary>
    public RgbColor Escape { get; }

    /// <summary>The planet explosion's and the flyaway's backdrop.</summary>
    public RgbColor Explosion { get; }

    /// <summary>The staff credits' backdrop.</summary>
    public RgbColor Credits { get; }

    /// <summary>The post-credits scenes' backdrop.</summary>
    public RgbColor PostCredits { get; }
}
