using System.Collections.Frozen;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Constants used by the native $90:C157 Power Bomb fuse routine.</summary>
public static class SamusPowerBombFuseData
{
    /// <summary>$90:C157 switches to the fast animation when the decremented fuse reaches fifteen.</summary>
    public const ushort FastAnimationTimer = 15;
    /// <summary>
    /// The fast-animation position each slow bomb and Power Bomb position switches to: the same frame in the fast list.
    /// A bomb's list has one more leading step than its fast list.
    /// </summary>
    internal static readonly FrozenDictionary<AnimationProgramCursor<SamusProjectileProgram>, AnimationProgramCursor<SamusProjectileProgram>> FastCounterparts =
        new Dictionary<AnimationProgramCursor<SamusProjectileProgram>, AnimationProgramCursor<SamusProjectileProgram>>
        {
            [new(SamusProjectileProgram.PowerBomb, 0)] = new(SamusProjectileProgram.FastPowerBomb, 0),
            [new(SamusProjectileProgram.PowerBomb, 1)] = new(SamusProjectileProgram.FastPowerBomb, 1),
            [new(SamusProjectileProgram.PowerBomb, 2)] = new(SamusProjectileProgram.FastPowerBomb, 2),
            [new(SamusProjectileProgram.PowerBomb, 3)] = new(SamusProjectileProgram.FastPowerBomb, 3),
            [new(SamusProjectileProgram.Bomb, 1)] = new(SamusProjectileProgram.FastBomb, 0),
            [new(SamusProjectileProgram.Bomb, 2)] = new(SamusProjectileProgram.FastBomb, 1),
            [new(SamusProjectileProgram.Bomb, 3)] = new(SamusProjectileProgram.FastBomb, 2),
            [new(SamusProjectileProgram.Bomb, 4)] = new(SamusProjectileProgram.FastBomb, 3),
        }.ToFrozenDictionary();
    /// <summary>Fuse-expiration marker consumed separately by the subsequent bank-$94 collision routine.</summary>
    public const ushort ExplosionStartedSentinel = ushort.MaxValue;
}
