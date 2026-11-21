using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The final logo (#2541): its actor, from the end of the white flash on, while it lands and fades in and while the
/// item-percentage text moves down below it.
/// </summary>
/// <param name="logo">The logo's actor.</param>
/// <param name="phase">The phase the scene runs: the logo's own, or the item percentage's move down below it.</param>
internal sealed partial class EndingLogoScene(EndingLogo logo, EndingCreditsPhase phase) : EndingScene
{
    /// <summary>The logo's actor.</summary>
    internal EndingLogo Logo { get; } = Ensure.NotNull(logo);

    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase { get; } =
        phase == EndingCreditsPhase.PostCreditsLogo || phase == EndingCreditsPhase.ItemPercentageScrollDown ? phase
            : throw new ArgumentException($"The final logo runs no {phase} phase.", nameof(phase));

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
            Logo.Draw(sprites, EndingCreditsPresentation.ActorArt(ending, CinematicSpriteFamily.EndingLogo));
    }

    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureLogo(ending, Logo, paint);
}
