namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The pictures a Tourian boss statue's animation shows. The idle animation dims the statue's eyes from bright to dark
/// and brightens them again, the release dims them once more, and the released statue, its eyes gone, stays through the
/// eye-glow and soul waits.
/// </summary>
public enum TourianStatueBlockPicture
{
    /// <summary>The statue with its eyes bright: the idle animation's turning point.</summary>
    EyesBright,

    /// <summary>The statue with its eyes dimmed, shown on the way to dark and back and first in the release.</summary>
    EyesDim,

    /// <summary>The statue with its eyes dark: the idle animation's far turning point and the release's second frame.</summary>
    EyesDark,

    /// <summary>The released statue, its eyes gone, held while the eye glows and the soul leaves.</summary>
    Released,
}
