namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The reactions of the Skree's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class SkreeReactions
{
    /// <summary>The reaction of the Skree's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Skree.Shot");
}
