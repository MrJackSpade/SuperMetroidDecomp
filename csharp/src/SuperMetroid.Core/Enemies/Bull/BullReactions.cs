namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The reactions of the Bull's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class BullReactions
{
    /// <summary>The reaction of the Bull's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Bull.Shot");
}
