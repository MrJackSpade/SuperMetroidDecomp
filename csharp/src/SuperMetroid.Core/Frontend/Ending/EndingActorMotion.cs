using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// How one ending actor moves on each of its updates, before its animation steps (#2489): each motion keeps whatever state
/// its own movement needs, and an actor's role names the motion it starts with (<see cref="EndingSpriteRoleDefinitions"/>).
/// </summary>
internal abstract partial class EndingActorMotion
{
    /// <summary>Moves <paramref name="actor"/> for this update in <paramref name="scene"/>, or deletes it when its motion ends it.</summary>
    internal abstract void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene);

    /// <summary>
    /// Answers <paramref name="cue"/>, which the actor's own animation raises for its motion (#2541): only the
    /// right-hand starfield's motion takes one, the cue that has it wait for the flyaway.
    /// </summary>
    /// <exception cref="InvalidOperationException">The motion takes no such cue.</exception>
    internal virtual void TakeCue(EndingSceneCue cue) =>
        throw new InvalidOperationException($"The {GetType().Name} motion takes no {cue} cue.");
}
