namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The reactions of a corpse's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class CorpseReactions
{
    /// <summary>The reaction of a corpse's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Corpse.Touch");

    /// <summary>The reaction of a corpse's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Corpse.Shot");
}
