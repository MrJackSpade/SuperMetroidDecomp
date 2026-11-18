using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The motion of a layer of the exploding planet, its glow, lava or first stars (#2489): the layer follows the planet's
/// animation, not a timer of its own, and leaves with the planet.
/// </summary>
internal sealed class EndingExplosionLayerMotion : EndingActorMotion
{
    /// <summary>Deletes the layer once the exploding planet is no longer shown.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene)
    {
        Ensure.NotNull(actor);
        if (!Ensure.NotNull(scene).ShowsExplodingPlanet)
            actor.Delete();
    }
}
