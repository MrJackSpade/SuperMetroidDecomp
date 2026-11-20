using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The staff credits (#2541): their scroll, which the credits' setup starts.</summary>
/// <param name="scroll">The staff credits' scroll.</param>
internal sealed class EndingCreditsRollScene(CreditsScroll scroll) : EndingScene
{
    /// <summary>The staff credits' scroll.</summary>
    private CreditsScroll Scroll { get; } = Ensure.NotNull(scroll);

    /// <inheritdoc/>
    internal override EndingCreditsPhase Phase => EndingCreditsPhase.Credits;

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        ending.StepCredits(Scroll);
        return true;
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows) =>
        EndingCreditsPresentation.DrawCast(ending, sprites, shows);

    /// <inheritdoc/>
    internal override LayeredScene CaptureScreen(EndingCreditsState ending, FramePaint paint) =>
        new(paint, [new PicturedPlaneLayer(ending.CreditsPlane(), Offset16<PlanePixels>.Of(0, Scroll.ScrolledPixels))], ending.Stage.Brightness);
}
