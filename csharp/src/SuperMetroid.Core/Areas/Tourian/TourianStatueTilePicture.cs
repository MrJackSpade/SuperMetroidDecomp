namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The pictures a Tourian boss statue's animated tiles show. The idle animation swings through the three oscillation
/// pictures forward and back, the release replays the second and third, and the released picture stays through the
/// eye-glow and soul waits.
/// </summary>
public enum TourianStatueTilePicture
{
    /// <summary>The first oscillation picture: the idle animation's turning point.</summary>
    OscillationFirst,

    /// <summary>The second oscillation picture, shown on the way out and back and first in the release.</summary>
    OscillationSecond,

    /// <summary>The third oscillation picture: the idle animation's far turning point and the release's second frame.</summary>
    OscillationThird,

    /// <summary>The released statue, held while the eye glows and the soul leaves.</summary>
    Released,
}
