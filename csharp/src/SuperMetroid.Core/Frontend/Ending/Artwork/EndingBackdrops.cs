using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The solid colour each ending scene shows behind every layer once its own colours show (#1544), and the backdrops of the
/// space behind the exploding planet (#1637), as the import supplies them. The ending loads no colour table: every picture
/// it draws is pre-coloured, and only these colours are its own.
/// </summary>
public sealed class EndingBackdrops
{
    /// <summary>Keeps each scene's backdrop.</summary>
    /// <param name="escape">The escape scenes' backdrop.</param>
    /// <param name="explosion">The planet explosion's and the flyaway's backdrop.</param>
    /// <param name="credits">The staff credits' backdrop.</param>
    /// <param name="postCredits">The post-credits scenes' backdrop.</param>
    /// <param name="space">The backdrops of the space behind the exploding planet, one for each frame of its colours.</param>
    public EndingBackdrops(RgbColor escape, RgbColor explosion, RgbColor credits, RgbColor postCredits, EndingSpaceBackdrops space)
    {
        Escape = escape;
        Explosion = explosion;
        Credits = credits;
        PostCredits = postCredits;
        Space = Ensure.NotNull(space);
    }

    /// <summary>The escape scenes' backdrop.</summary>
    public RgbColor Escape { get; }

    /// <summary>The planet explosion's and the flyaway's backdrop.</summary>
    public RgbColor Explosion { get; }

    /// <summary>The staff credits' backdrop.</summary>
    public RgbColor Credits { get; }

    /// <summary>The post-credits scenes' backdrop.</summary>
    public RgbColor PostCredits { get; }

    /// <summary>The backdrops of the space behind the exploding planet, one for each frame of its colours.</summary>
    public EndingSpaceBackdrops Space { get; }
}
