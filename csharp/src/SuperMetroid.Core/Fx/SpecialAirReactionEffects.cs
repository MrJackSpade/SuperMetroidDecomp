namespace SuperMetroid.Core.Fx;

/// <summary>What one special-air reaction does to Samus inside its block and when her movement reaches it (#2571).</summary>
/// <param name="Inside">What the block does to Samus inside it.</param>
/// <param name="Collision">What the block does when her movement reaches it.</param>
internal readonly record struct SpecialAirReactionEffects(SpecialAirInsideReaction Inside, SpecialAirCollisionReaction Collision);
