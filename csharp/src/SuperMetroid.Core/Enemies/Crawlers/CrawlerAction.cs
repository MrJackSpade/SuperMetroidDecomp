namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The behaviour a creepy-crawly enemy's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum CrawlerAction
{
    /// <summary>Return-only initial orange-Zoomer state, awaiting the animation command that installs a horizontal or vertical crawling action.</summary>
    HZoomerAwaitingAnimationCue = 1,
    /// <summary>Maintains wall contact and traverses vertical edges, reversing its tangent toward Samus while attached.</summary>
    HZoomerCrawlingVertically,
    /// <summary>Maintains floor or ceiling contact and traverses edges, reversing its horizontal tangent toward Samus while attached.</summary>
    HZoomerCrawlingHorizontally,
    /// <summary>Return-only shared-crawler initial state, awaiting the surface animation's command to install the crawling action.</summary>
    AwaitingAnimationCue,
    /// <summary>Shared wall-crawling behaviour that probes horizontal attachment, advances vertically, and rotates the surface animation at inside or outside corners.</summary>
    CrawlingVertically,
    /// <summary>Shared detachment fall with half-pixel downward acceleration until the whole velocity reaches four pixels per update; landing clears fall motion and restores the saved attached actction.</summary>
    Falling,
    /// <summary>Shared floor/ceiling-crawling behaviour that probes vertical attachment, advances horizontally with slope adjustment, and rotates the surface animation at corners.</summary>
    CrawlingHorizontally,
}
