namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The behaviour a creepy-crawly enemy's main AI runs each frame; its entity decides how it crawls, the orange Zoomer by
/// its own pursuit (#2283). Its values are arbitrary.
/// </summary>
public enum CrawlerAction
{
    /// <summary>The starting state: the crawler settles for its first update, then crawls along the surface it starts on.</summary>
    Settling,
    /// <summary>
    /// Wall crawling: probes horizontal attachment, advances vertically and rotates the surface animation at inside or
    /// outside corners; an orange Zoomer also reverses its tangent toward Samus while attached.
    /// </summary>
    CrawlingVertically,
    /// <summary>Detachment fall with its downward gravity (<see cref="CrawlerMotionDefinitions.FallingGravity"/>) until the whole velocity reaches <see cref="CrawlerMotionDefinitions.FallingSpeedCap"/>; landing clears fall motion and restores the saved attached action.</summary>
    Falling,
    /// <summary>
    /// Floor or ceiling crawling: probes vertical attachment, advances horizontally with slope adjustment and rotates the
    /// surface animation at corners; an orange Zoomer also reverses its tangent toward Samus while attached.
    /// </summary>
    CrawlingHorizontally,
}
