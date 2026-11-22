using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The rescued animals' escape pod's motion (#2489): to the right and very slowly down each update, until it
/// leaves at the right of the screen.
/// </summary>
internal sealed partial class EndingAnimalPodMotion : EndingActorMotion
{
    /// <summary>Moves the pod and deletes it once its centre reaches <see cref="EndingAnimalEscapeDefinitions.DeleteX"/>.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene)
    {
        Ensure.NotNull(actor);
        actor.Position = new(
            actor.Position.X.WithWholePixels(actor.Position.X.WholePixels + EndingAnimalEscapeDefinitions.XVelocity),
            actor.Position.Y + EndingAnimalEscapeDefinitions.YVelocity);
        if (actor.ScreenPlace.X >= EndingAnimalEscapeDefinitions.DeleteX)
            actor.Delete();
    }
}
