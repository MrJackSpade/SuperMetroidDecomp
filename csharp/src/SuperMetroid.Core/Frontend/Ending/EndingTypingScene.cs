using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The completed logo's picture while one of the post-credits texts types beneath it (#2541, #2553): the item percentage,
/// then the final message, which the ending stays on. The logo's actors have finished and show nothing.
/// </summary>
/// <param name="typing">The text that types now.</param>
internal sealed class EndingTypingScene(EndingTypewriter typing) : EndingScene
{
    /// <summary>The text that types now.</summary>
    internal EndingTypewriter Typing { get; } = Ensure.NotNull(typing);

    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase =>
        Typing.TypesItemPercentage ? EndingCreditsPhase.ItemPercentage : EndingCreditsPhase.SeeYouNextMission;

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepTyping(this);
        return true;
    }

    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureLogo(ending, SpriteBlend.Glows, paint);
}
