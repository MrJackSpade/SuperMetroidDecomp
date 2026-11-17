using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's death: how long he waits before he falls and how fast he falls, how long his wall bursts, how many drops it
/// leaves, and where his drops fall and where and how often his wall explosions burst.
/// </summary>
internal static class BotwoonDeathDefinitions
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
    internal static readonly QuadraticSpeedCurvePlace FallAcceleration = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(192);

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

    /// <summary>How far above the wall sequence's frame, before its jitter, a wall explosion bursts.</summary>
    internal const int ExplosionRise = 8;

    /// <summary>The updates from one large wall explosion to the next once they run: one bursts every thirteenth update.</summary>
    internal const int LargeExplosionInterval = 13;

    /// <summary>The updates from one pair of small wall explosions to the next once they run: a pair bursts every fifth update.</summary>
    internal const int SmallExplosionInterval = 5;

    /// <summary>A death drop's X across its 128-pixel strip from X 64.</summary>
    internal static readonly RandomSampleField DropX = new(0, 7);

    /// <summary>A death drop's Y down its 64-pixel band from Y 128.</summary>
    internal static readonly RandomSampleField DropY = new(8, 6);

    /// <summary>A large wall explosion's X across its 32-pixel strip from X 232.</summary>
    internal static readonly RandomSampleField LargeExplosionX = new(0, 5);

    /// <summary>A small wall explosion's X across its 64-pixel strip from X 224.</summary>
    internal static readonly RandomSampleField SmallExplosionX = new(0, 6);

    /// <summary>
    /// A wall explosion's height jitter, 0 through 31 pixels added to the wall sequence's frame less eight, drawn for large and small explosions alike.
    /// </summary>
    internal static readonly RandomSampleField ExplosionYJitter = new(0, 5);
}
