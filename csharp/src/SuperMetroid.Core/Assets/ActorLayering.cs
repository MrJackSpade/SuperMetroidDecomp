namespace SuperMetroid.Core.Assets;

/// <summary>Where an actor's picture shows among the actors drawn before it in a frame (#2573).</summary>
public enum ActorLayering
{
    /// <summary>In draw order: behind the actors drawn before it where they overlap, at the same depth.</summary>
    InDrawOrder,

    /// <summary>In front of every actor drawn before it, at the same depth.</summary>
    InFrontOfActors,
}
