using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses;

/// <summary>The hit and reaction of one hitbox result.</summary>
/// <param name="Hit">The hit.</param>
/// <param name="Reaction">The selected reaction, or null when none was selected.</param>
internal readonly record struct HitboxResult(bool Hit, EnemyReaction? Reaction);
