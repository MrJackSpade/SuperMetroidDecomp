using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// How one ending actor moves on each of its updates, before its animation steps (#2489): each motion keeps whatever state
/// its own movement needs, and an actor's role names the motion it starts with (<see cref="EndingSpriteRoleDefinitions"/>).
/// </summary>
internal abstract class EndingActorMotion
{
    /// <summary>Moves <paramref name="actor"/> for this update in <paramref name="scene"/>, or deletes it when its motion ends it.</summary>
    internal abstract void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene);
}
