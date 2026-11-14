using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The reactions of the Maridia large snail's own that its frames' hitboxes name (#1759). Only the Maridia large snail's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class MaridiaLargeSnailReactions
{
    /// <summary>The touch reaction that damages Samus.</summary>
    internal static EnemyReaction DamagingTouch { get; } = EnemyReaction.OfSpecies("MaridiaLargeSnail.DamagingTouch");

    /// <summary>The touch reaction that leaves Samus unharmed.</summary>
    internal static EnemyReaction NonDamagingTouch { get; } = EnemyReaction.OfSpecies("MaridiaLargeSnail.NonDamagingTouch");

    /// <summary>The snail's shot reaction.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("MaridiaLargeSnail.Shot");
}
