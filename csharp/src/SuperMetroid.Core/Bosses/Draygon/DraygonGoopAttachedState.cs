using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What a blob of Draygon's goop stuck to Samus keeps: where in the stack on her it sits, and how long it stays.</summary>
/// <param name="stackPosition">How many blobs, this one included, were stuck to Samus when it stuck; it sets how far down her it sits.</param>
internal sealed class DraygonGoopAttachedState(int stackPosition) : EnemyProjectileState
{
    /// <summary>Updates a blob stays stuck before it falls off.</summary>
    private const int AttachedLifetime = 256;

    /// <summary>How many blobs, this one included, were stuck to Samus when it stuck; each sits four pixels below the last.</summary>
    internal int StackPosition { get; } = Ensure.AtLeastZero(stackPosition);

    /// <summary>The blob's remaining time stuck; it falls off on the update this reaches zero.</summary>
    internal WrappingCountdown Lifetime { get; set; } = WrappingCountdown.Start(AttachedLifetime);
}
