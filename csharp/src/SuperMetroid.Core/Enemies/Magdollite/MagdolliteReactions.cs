namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The reactions of the Magdollite's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static class MagdolliteReactions
{
    /// <summary>The reaction of the Magdollite's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Magdollite.Touch");

    /// <summary>The reaction of the Magdollite's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Magdollite.Shot");
}
