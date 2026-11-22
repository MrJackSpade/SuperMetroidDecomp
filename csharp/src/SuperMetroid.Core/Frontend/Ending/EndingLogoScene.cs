using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The final logo (#2541): its actor, from the end of the white flash on, while it lands and fades in, until it completes.
/// </summary>
/// <param name="logo">The logo's actor.</param>
internal sealed partial class EndingLogoScene(EndingLogo logo) : EndingActorScene
{
    /// <summary>The logo's actor.</summary>
    internal EndingLogo Logo { get; } = Ensure.NotNull(logo);

    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase => EndingCreditsPhase.PostCreditsLogo;

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepLogo(this);
        return true;
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows)
    {
        if (shows(EndingSpriteLayer.Actors))
            Logo.Draw(sprites, EndingCreditsPresentation.ActorArt(ending, EndingSpriteFamily.EndingLogo));
    }

    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureLogo(ending, Logo.Sprites, paint);
}
