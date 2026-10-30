using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// What a Polyp's rock keeps: whether it is still rising or has passed the top of its arc, and where on the shared
/// quadratic speed table its vertical speed stands.
/// </summary>
/// <param name="speedIndex">The rock's starting vertical speed, as a position on the quadratic speed table.</param>
internal sealed class PolypRockState(int speedIndex) : EnemyProjectileState
{
    /// <summary>The part of its arc the rock is in; it starts rising.</summary>
    internal PolypRockFunction Phase { get; set; } = PolypRockFunction.Rising;

    /// <summary>
    /// The rock's vertical speed as a position on the quadratic speed table: it falls by two each update while the
    /// rock rises and grows by two, up to a cap, while it falls.
    /// </summary>
    internal int SpeedIndex
    {
        get;
        set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value);
    } = speedIndex;
}
