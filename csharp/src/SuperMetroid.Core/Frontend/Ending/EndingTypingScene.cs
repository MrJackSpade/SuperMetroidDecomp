using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The final logo while one of the post-credits texts types beneath it (#2541): the item percentage, then the final
/// message, which the ending stays on.
/// </summary>
/// <param name="logo">The logo's actor.</param>
/// <param name="typing">The text that types now.</param>
internal sealed class EndingTypingScene(EndingLogo logo, EndingTypewriter typing) : EndingScene
{
    /// <summary>The logo's actor.</summary>
    internal EndingLogo Logo { get; } = Ensure.NotNull(logo);

    /// <summary>The text that types now.</summary>
    internal EndingTypewriter Typing { get; } = Ensure.NotNull(typing);

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepTyping(this);
        return true;
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows)
    {
        if (shows(EndingSpriteLayer.Actors))
            Logo.Draw(sprites, EndingCreditsPresentation.ActorArt(ending, CinematicSpriteFamily.EndingLogo));
    }

    /// <inheritdoc/>
    internal override LayeredScene? CaptureOwnScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureLogo(ending, Logo, paint);
}
