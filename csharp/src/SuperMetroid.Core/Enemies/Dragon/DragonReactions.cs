namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The reactions of the Dragon's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class DragonReactions
{
    /// <summary>The reaction of the Dragon's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Dragon.Touch");

    /// <summary>The reaction of the Dragon's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Dragon.Shot");
}
