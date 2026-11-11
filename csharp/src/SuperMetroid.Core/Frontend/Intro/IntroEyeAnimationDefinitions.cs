namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Where the portrait's eye is drawn. The simulation runs <see cref="IntroEyeAnimations"/> and draws each of its frames
/// at <see cref="PortraitEyeCorner"/>.
/// </summary>
internal static class IntroEyeAnimationDefinitions
{
    /// <summary>The portrait plane pixel every eye frame's upper-left corner is drawn at.</summary>
    internal static Position16<PlanePixels> PortraitEyeCorner => new(136, 104);
}
