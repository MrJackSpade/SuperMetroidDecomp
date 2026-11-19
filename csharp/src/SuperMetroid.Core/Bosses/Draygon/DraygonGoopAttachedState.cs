using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What a blob of Draygon's goop stuck to Samus keeps: where in the stack on her it sits, and how long it stays.</summary>
/// <param name="stackPosition">How many blobs, this one included, were stuck to Samus when it stuck; it sets how far down her it sits.</param>
internal sealed class DraygonGoopAttachedState(int stackPosition) : EnemyProjectileState
{
    /// <summary>How many blobs, this one included, were stuck to Samus when it stuck; each sits four pixels below the last.</summary>
    internal int StackPosition { get; } = Ensure.AtLeastZero(stackPosition);

    /// <summary>
    /// The blob's time stuck to Samus (#2538): it lasts <see cref="DraygonArenaDefinitions.GoopAttachedLifetime"/> from
    /// the update it sticks, and the blob falls off on its last update.
    /// </summary>
    internal BossCountdown Lifetime { get; } = BossCountdown.Lasting(DraygonArenaDefinitions.GoopAttachedLifetime);
}
