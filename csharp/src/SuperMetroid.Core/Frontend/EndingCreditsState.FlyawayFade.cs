using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private byte flyawayWhite;
    private int flyawayFadeCountdown;
    private FixedColorAddRenderLayer? FlyawayFixedColor => flyawayWhite != 0
        && Phase >= EndingCreditsPhase.PlanetEscapeFast && Phase <= EndingCreditsPhase.FadeOutToCredits
        ? new(flyawayWhite, flyawayWhite, flyawayWhite) : null;

    private void StepFlyawayFade()
    {
        if (--flyawayFadeCountdown > 0) return;
        if (flyawayWhite > 0) flyawayWhite--;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.Interval;
    }
}
