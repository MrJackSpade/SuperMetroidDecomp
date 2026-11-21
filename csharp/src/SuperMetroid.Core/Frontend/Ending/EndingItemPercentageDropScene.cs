using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The completed logo's picture while the typed item percentage moves down beneath it (#2553), before the final message
/// types. The logo's actors have finished and show nothing.
/// </summary>
internal sealed class EndingItemPercentageDropScene : EndingScene
{
    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase => EndingCreditsPhase.ItemPercentageScrollDown;

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepItemPercentageDrop();
        return true;
    }

    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        EndingCreditsPresentation.CaptureLogo(ending, SpriteBlend.Glows, paint);
}
