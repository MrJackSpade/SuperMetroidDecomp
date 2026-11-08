using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Kraid arm hitbox list: how Samus touching it reacts, and its rectangles.</summary>
/// <param name="Touch">The reaction to Samus touching any of its rectangles.</param>
/// <param name="Rectangles">Its rectangles in collision order; the first overlapping one wins.</param>
internal readonly record struct KraidHitboxListDefinition(EnemyReaction Touch, ImmutableArray<EnemyHitboxRectangle> Rectangles);
