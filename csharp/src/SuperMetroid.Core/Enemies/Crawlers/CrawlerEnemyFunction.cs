namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Bank-$A3 function pointer stored in a creepy-crawly enemy's native variable F.</summary>
public enum CrawlerEnemyFunction : ushort
{
    /// <summary>$A3:E08A: return-only initial orange-Zoomer state, awaiting the animation command that installs a horizontal or vertical crawling function.</summary>
    HZoomerInstructionPending = 0xe08a,
    /// <summary>$A3:E091, <c>Function_HZoomer_CrawlingVertically</c>: maintains wall contact and traverses vertical edges, reversing its tangent toward Samus while attached.</summary>
    HZoomerCrawlingVertically = 0xe091,
    /// <summary>$A3:E168, <c>Function_HZoomer_CrawlingHorizontally</c>: maintains floor or ceiling contact and traverses edges, reversing its horizontal tangent toward Samus while attached.</summary>
    HZoomerCrawlingHorizontally = 0xe168,
    /// <summary>$A3:E6C1: return-only shared-crawler initial state, awaiting the surface animation's command to install the crawling function.</summary>
    InstructionPending = 0xe6c1,
    /// <summary>$A3:E6C8: shared wall-crawling routine that probes horizontal attachment, advances vertically, and rotates the surface animation at inside or outside corners.</summary>
    CrawlingVertically = 0xe6c8,
    /// <summary>$A3:E785: shared detachment fall with half-pixel downward acceleration until the whole velocity reaches four pixels per update; landing clears fall motion and restores the saved attached function.</summary>
    Falling = 0xe785,
    /// <summary>$A3:E7F2: shared floor/ceiling-crawling routine that probes vertical attachment, advances horizontally with slope adjustment, and rotates the surface animation at corners.</summary>
    CrawlingHorizontally = 0xe7f2,
}
