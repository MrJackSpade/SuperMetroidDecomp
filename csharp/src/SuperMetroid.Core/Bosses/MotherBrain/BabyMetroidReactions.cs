using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The reactions of the Baby Metroid's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static partial class BabyMetroidReactions
{
    /// <summary>The reaction of the Baby Metroid's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch => SpeciesOwnReactions.BabyMetroidTouch;

    /// <summary>The reaction of the Baby Metroid's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot => SpeciesOwnReactions.BabyMetroidShot;
}
