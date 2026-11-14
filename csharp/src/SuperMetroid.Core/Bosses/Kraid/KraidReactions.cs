using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The reactions of Kraid's own that its frames' hitboxes name (#1759). Only Kraid's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class KraidReactions
{
    /// <summary>The touch reaction of Kraid's background and feet.</summary>
    internal static EnemyReaction BackgroundTouch { get; } = EnemyReaction.OfSpecies("Kraid.BackgroundTouch");

    /// <summary>The arm's touch reaction, which pushes Samus away.</summary>
    internal static EnemyReaction ArmTouch { get; } = EnemyReaction.OfSpecies("Kraid.ArmTouch");

    /// <summary>The arm's shot reaction.</summary>
    internal static EnemyReaction ArmShot { get; } = EnemyReaction.OfSpecies("Kraid.ArmShot");
}
