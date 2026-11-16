using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's death: how long he waits before he falls and how fast he falls, how long his wall bursts, how many drops it
/// leaves, and where his drops fall and where and how often his wall explosions burst.
/// </summary>
internal static class BotwoonDeathDefinitions
{
    /// <summary>The count Botwoon's wait before his fall starts from once his tail has finished entering its hole.</summary>
    internal const int DeathDelayStart = 240;

    /// <summary>The count of the wait at which his head starts falling.</summary>
    internal const int DeathDelayEnd = 256;

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

    /// <summary>The updates between large wall explosions once they run.</summary>
    internal const int LargeExplosionInterval = 12;

    /// <summary>The updates between pairs of small wall explosions once they run.</summary>
    internal const int SmallExplosionInterval = 4;

    /// <summary>A death drop's X across its 128-pixel strip from X 64: the sample modulo 128.</summary>
    internal static readonly RandomSampleField DropX = new(0, 7);

    /// <summary>A death drop's Y down its 64-pixel band from Y 128: the sample divided by 256, modulo 64.</summary>
    internal static readonly RandomSampleField DropY = new(8, 6);

    /// <summary>A large wall explosion's X across its 32-pixel strip from X 232: the sample modulo 32.</summary>
    internal static readonly RandomSampleField LargeExplosionX = new(0, 5);

    /// <summary>A small wall explosion's X across its 64-pixel strip from X 224: the sample modulo 64.</summary>
    internal static readonly RandomSampleField SmallExplosionX = new(0, 6);

    /// <summary>
    /// A wall explosion's height jitter, 0 through 31 pixels added to the wall sequence's frame less eight: the sample
    /// modulo 32, drawn for large and small explosions alike.
    /// </summary>
    internal static readonly RandomSampleField ExplosionYJitter = new(0, 5);
}
