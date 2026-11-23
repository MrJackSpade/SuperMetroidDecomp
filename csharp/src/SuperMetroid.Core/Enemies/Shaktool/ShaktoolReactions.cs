namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The reactions of the Shaktool's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class ShaktoolReactions
{
    /// <summary>The reaction of the Shaktool's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Shaktool.Touch");

    /// <summary>The reaction of the Shaktool's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Shaktool.Shot");
}
