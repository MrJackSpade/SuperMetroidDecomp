namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The reactions of the Oum's own that its frames' hitboxes name (#1759). Only the Oum's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class OumReactions
{
    /// <summary>The touch reaction that damages Samus.</summary>
    internal static EnemyReaction DamagingTouch { get; } = EnemyReaction.OfSpecies("Oum.DamagingTouch");

    /// <summary>The touch reaction that leaves Samus unharmed.</summary>
    internal static EnemyReaction NonDamagingTouch { get; } = EnemyReaction.OfSpecies("Oum.NonDamagingTouch");

    /// <summary>The snail's shot reaction.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Oum.Shot");
}
