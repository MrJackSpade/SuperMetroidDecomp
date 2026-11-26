using Engine.Collision;
using Engine.Shared;

namespace Engine.World;

/// <summary>Something a hit can land on. It alone decides what the hit does to it.</summary>
public interface IHittable
{
    /// <summary>Whether hits from <paramref name="side"/> can land on it at all.</summary>
    bool IsHitBy(HitSide side);

    /// <summary>Takes a hit that landed on <paramref name="part"/>, one of its hurtboxes, and says what it did.</summary>
    HitResult TakeHit(Hit hit, Collider part);

    /// <summary>
    /// Whether what delivered <paramref name="hit"/>, and would pass through it, stops on it instead: a shot that
    /// goes on through what it hurts ends on this as a shot that does not. No, unless it says so.
    /// </summary>
    bool Stops(Hit hit) => false;
}
