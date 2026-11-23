namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The reactions of the Mochtroid's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class MochtroidReactions
{
    /// <summary>The reaction of the Mochtroid's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Mochtroid.Touch");

    /// <summary>The reaction of the Mochtroid's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Mochtroid.Shot");
}
