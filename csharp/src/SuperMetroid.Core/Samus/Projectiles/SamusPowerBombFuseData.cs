using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Constants used by the Power Bomb fuse.</summary>
public static class SamusPowerBombFuseData
{
    /// <summary>The fuse switches to the fast animation when the decremented fuse reaches fifteen.</summary>
    public const int FastAnimationTimer = 15;
    /// <summary>
    /// The fast-animation place each slow bomb and Power Bomb place switches to: the same frame in the fast segment.
    /// A bomb's segment is one beat ahead of its fast segment.
    /// </summary>
    internal static readonly FrozenDictionary<SamusProjectileSegmentBeat, SamusProjectileSegmentBeat> FastCounterparts =
        new Dictionary<SamusProjectileSegmentBeat, SamusProjectileSegmentBeat>
        {
            [new(SamusProjectileSegment.PowerBomb, 0)] = new(SamusProjectileSegment.FastPowerBomb, 0),
            [new(SamusProjectileSegment.PowerBomb, 1)] = new(SamusProjectileSegment.FastPowerBomb, 1),
            [new(SamusProjectileSegment.PowerBomb, 2)] = new(SamusProjectileSegment.FastPowerBomb, 2),
            [new(SamusProjectileSegment.PowerBomb, 3)] = new(SamusProjectileSegment.FastPowerBomb, 3),
            [new(SamusProjectileSegment.Bomb, 1)] = new(SamusProjectileSegment.FastBomb, 0),
            [new(SamusProjectileSegment.Bomb, 2)] = new(SamusProjectileSegment.FastBomb, 1),
            [new(SamusProjectileSegment.Bomb, 3)] = new(SamusProjectileSegment.FastBomb, 2),
            [new(SamusProjectileSegment.Bomb, 4)] = new(SamusProjectileSegment.FastBomb, 3),
        }.ToFrozenDictionary();
}
