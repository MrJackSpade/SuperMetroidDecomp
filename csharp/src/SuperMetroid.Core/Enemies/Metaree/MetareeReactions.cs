namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The reactions of the Metaree's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class MetareeReactions
{
    /// <summary>The reaction of the Metaree's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Metaree.Shot");
}
