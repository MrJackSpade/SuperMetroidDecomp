using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The motion of the exploded planet's afterglow (#2489): it stands still and leaves at the handoff out of the explosion's
/// last phase, as the completion text starts.
/// </summary>
internal sealed partial class EndingFinaleExitMotion : EndingActorMotion
{
    /// <summary>Deletes the actor once the completion text starts.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene)
    {
        Ensure.NotNull(actor);
        if (EndingCreditsPhaseGroups.CompletionText.Contains(Ensure.NotNull(scene).Phase))
            actor.Delete();
    }
}
