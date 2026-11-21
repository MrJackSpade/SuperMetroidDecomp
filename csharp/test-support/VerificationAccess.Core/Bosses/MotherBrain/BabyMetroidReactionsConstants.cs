using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="BabyMetroidReactions"/> that only verification reads.</summary>
internal static class BabyMetroidReactionsConstants
{
    /// <summary>The reaction of the Baby Metroid's own to Samus's touch in Mother Brain's cutscene, which its entity answers.</summary>
    internal static EnemyReaction CutsceneTouch => SpeciesOwnReactions.BabyMetroidCutsceneTouch;
}
