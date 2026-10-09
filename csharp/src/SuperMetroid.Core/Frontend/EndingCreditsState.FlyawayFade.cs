using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Current equal-channel white intensity for the flyaway fade, reduced at each fade interval.</summary>
    private byte flyawayWhite;
    /// <summary>Remaining update count before the current white intensity is reduced again.</summary>
    private int flyawayFadeCountdown;

    /// <summary>Produces the flyaway white color only while intensity is nonzero and the escape or credits fade is active.</summary>
    private FixedColorAddRenderLayer? FlyawayFixedColor => flyawayWhite != 0
        && Phase >= EndingCreditsPhase.PlanetEscapeFast && Phase <= EndingCreditsPhase.FadeOutToCredits
        ? new(flyawayWhite, flyawayWhite, flyawayWhite) : null;

    /// <summary>Advances the flyaway fade timer and lowers its white intensity whenever the configured interval expires.</summary>
    private void StepFlyawayFade()
    {
        if (--flyawayFadeCountdown > 0) return;
        if (flyawayWhite > 0) flyawayWhite--;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.Interval;
    }
}
