namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The reactions of a Ki-Hunter's body's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class KiHunterReactions
{
    /// <summary>The reaction of a Ki-Hunter's body's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("KiHunter.Shot");
}
