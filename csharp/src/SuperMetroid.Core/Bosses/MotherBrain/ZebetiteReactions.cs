using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The reactions of a Zebetite's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class ZebetiteReactions
{
    /// <summary>The reaction of a Zebetite's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch => SpeciesOwnReactions.ZebetiteTouch;

    /// <summary>The reaction of a Zebetite's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot => SpeciesOwnReactions.ZebetiteShot;
}
