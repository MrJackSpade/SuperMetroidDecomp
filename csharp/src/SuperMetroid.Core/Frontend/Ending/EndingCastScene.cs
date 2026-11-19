using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's cast in its running order (#2541): the escape's clouds, the explosion's actors, the completion text, the
/// waiting backdrops and the idle reward, which the ending's stage holds and runs, with no actor of the scene's own. The
/// scene shows one of the cast's phases (#2571), and each cast phase the ending enters shows a scene of its own.
/// </summary>
/// <param name="phase">The cast's phase the scene shows.</param>
internal sealed class EndingCastScene(EndingCastPhase phase) : EndingScene
{
    /// <summary>The cast's phase the scene shows.</summary>
    internal EndingCastPhase Phase { get; } = Ensure.NotNull(phase);

    /// <inheritdoc/>
    internal override bool Step(EndingCreditsState ending)
    {
        // Each setup wait ends the update inside the setup phase, before the phase's own work; the update after the last
        // wait runs the scene setup.
        return Phase.Step(ending);
    }

    /// <inheritdoc/>
    internal override void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows) =>
        EndingCreditsPresentation.DrawCast(ending, sprites, shows);
}
