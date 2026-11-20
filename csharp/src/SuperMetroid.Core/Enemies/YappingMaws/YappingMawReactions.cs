namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The reactions of the Yapping Maw's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static class YappingMawReactions
{
    /// <summary>The reaction of the Yapping Maw's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("YappingMaw.Touch");

    /// <summary>The reaction of the Yapping Maw's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("YappingMaw.Shot");
}
