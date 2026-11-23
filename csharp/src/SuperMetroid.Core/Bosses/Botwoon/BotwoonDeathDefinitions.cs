using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's death: how long he waits before he falls and how fast he falls, how long his wall bursts, how many drops it
/// leaves, and where his drops fall and where and how often his wall explosions burst.
/// </summary>
internal static partial class BotwoonDeathDefinitions
{
    /// <summary>
    /// The updates Botwoon waits once his tail has finished entering the wall after the fatal hit, the update it does so
    /// included; his head starts falling on the update after.
    /// </summary>
    internal const int PreFallWaitUpdates = 16;

    /// <summary>
    /// How far along the quadratic speed curve the dying head and each falling body segment move each airborne update:
    /// three quarters of a mark.
    /// </summary>
    internal static readonly QuadraticSpeedCurvePlace FallAcceleration = QuadraticSpeedCurvePlace.FromMarkFraction(3, 4);

    /// <summary>The updates the wall's explosions last before Botwoon is removed.</summary>
    internal const int WallExplosionDuration = 192;

    /// <summary>The update of the wall's explosions from which the explosions burst.</summary>
    internal const int WallExplosionsBegin = 64;

    /// <summary>The drops the crumbling wall leaves, each chosen by Botwoon's drop chances.</summary>
    internal const int WallDropCount = 16;

    /// <summary>The left edge of the strip the death drops fall across.</summary>
    internal const int DropLeft = 64;

    /// <summary>The top of the band the death drops fall within.</summary>
    internal const int DropTop = 128;

    /// <summary>The left edge of the strip the large wall explosions burst across.</summary>
    internal const int LargeExplosionLeft = 232;

    /// <summary>The left edge of the strip the small wall explosions burst across.</summary>
    internal const int SmallExplosionLeft = 224;

    /// <summary>The small wall explosions each burst of them sets off (#2182).</summary>
    internal const int SmallExplosionsPerBurst = 2;

    /// <summary>How far above the room's top the band the wall explosions burst in starts, in pixels.</summary>
    internal const int ExplosionRise = 8;

    /// <summary>How far down the wall the band the wall explosions burst in moves each update of the sequence, in pixels.</summary>
    internal const int ExplosionBandDescent = 1;

    /// <summary>The updates from one large wall explosion to the next once they run.</summary>
    internal const int LargeExplosionInterval = 13;

    /// <summary>The updates from one pair of small wall explosions to the next once they run.</summary>
    internal const int SmallExplosionInterval = 5;

    /// <summary>A death drop's X across its strip from <see cref="DropLeft"/>.</summary>
    internal static readonly RandomSampleField DropX = RandomSampleField.OneOf(128);

    /// <summary>A death drop's Y down its band from <see cref="DropTop"/>.</summary>
    internal static readonly RandomSampleField DropY = RandomSampleField.SecondOf(64);

    /// <summary>A large wall explosion's X across its strip from <see cref="LargeExplosionLeft"/>.</summary>
    internal static readonly RandomSampleField LargeExplosionX = RandomSampleField.OneOf(32);

    /// <summary>A small wall explosion's X across its strip from <see cref="SmallExplosionLeft"/>.</summary>
    internal static readonly RandomSampleField SmallExplosionX = RandomSampleField.OneOf(64);

    /// <summary>
    /// A wall explosion's height jitter, in pixels below the top of the band it bursts in, drawn for large and small explosions alike.
    /// </summary>
    internal static readonly RandomSampleField ExplosionYJitter = RandomSampleField.OneOf(32);
}
