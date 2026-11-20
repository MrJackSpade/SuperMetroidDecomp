using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The reactions of Botwoon's own (#2558) that its definition names where it shares none of the engine's: the
/// reaction that runs when Samus or her weapons meet it, which its entity answers.
/// </summary>
internal static class BotwoonReactions
{
    /// <summary>The reaction of Botwoon's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch => SpeciesOwnReactions.BotwoonTouch;

    /// <summary>The reaction of Botwoon's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot => SpeciesOwnReactions.BotwoonShot;
}
