namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The reactions of the Yard's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static class YardReactions
{
    /// <summary>The reaction of the Yard's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Yard.Touch");

    /// <summary>The reaction of the Yard's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Yard.Shot");
}
