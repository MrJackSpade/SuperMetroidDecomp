using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The motion of an ending actor with no motion of its own (#2541): the exploding planet, the explosion's silhouette, the
/// completion texts and digits and the waiting reward stand where their animations place them.
/// </summary>
internal sealed class EndingStillMotion : EndingActorMotion
{
    /// <summary>Leaves <paramref name="actor"/> where it stands.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene) => Ensure.NotNull(actor);
}
