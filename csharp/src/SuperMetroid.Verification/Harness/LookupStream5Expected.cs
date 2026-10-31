using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>One Phantoon hitbox's four bounds and its touch and shot reactions.</summary>
/// <param name="First">The left bound.</param>
/// <param name="Second">The top bound.</param>
/// <param name="Third">The right bound.</param>
/// <param name="Fourth">The bottom bound.</param>
/// <param name="Fifth">The touch reaction.</param>
/// <param name="Sixth">The shot reaction.</param>
internal readonly record struct LookupStream5Expected(int First, int Second, int Third, int Fourth, EnemyReaction? Fifth, EnemyReaction? Sixth);
