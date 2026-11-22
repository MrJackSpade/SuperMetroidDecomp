namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cinematic actor definition: the animation it starts. Its motion belongs to its owner (#1903).</summary>
/// <typeparam name="TAnimation">The animation identity of the actor's family (#2197).</typeparam>
/// <param name="Animation">The animation it starts.</param>
internal readonly partial record struct CinematicActorDefinition<TAnimation>(TAnimation Animation)
    where TAnimation : struct, Enum;
