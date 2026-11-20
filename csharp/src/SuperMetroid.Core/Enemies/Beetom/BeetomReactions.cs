namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The reactions of the Beetom's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static class BeetomReactions
{
    /// <summary>The reaction of the Beetom's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Beetom.Touch");

    /// <summary>The reaction of the Beetom's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Beetom.Shot");
}
