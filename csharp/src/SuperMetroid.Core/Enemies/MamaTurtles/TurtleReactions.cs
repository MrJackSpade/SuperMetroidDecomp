namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The reactions of a turtle's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class TurtleReactions
{
    /// <summary>The reaction of a turtle's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Turtle.Touch");

    /// <summary>The reaction of a turtle's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Turtle.Shot");
}
