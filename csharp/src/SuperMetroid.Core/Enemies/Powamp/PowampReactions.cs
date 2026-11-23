namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The reactions of the Powamp's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class PowampReactions
{
    /// <summary>The reaction of the Powamp's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Powamp.Touch");

    /// <summary>The reaction of the Powamp's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Powamp.Shot");
}
