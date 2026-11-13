using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled NTSC beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>
    /// The horizontal and vertical speed every retail beam combination launches at, in 256ths of a pixel per update:
    /// four pixels.
    /// </summary>
    internal static readonly SignedEightEight CardinalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(1024);
    /// <summary>The diagonal speed every retail beam combination launches at, in 256ths of a pixel per update.</summary>
    internal static readonly SignedEightEight DiagonalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(683);
    /// <summary>The 8.8 base speed a fired missile ignites at: one pixel per update.</summary>
    internal static readonly SignedEightEight MissileIgnitionSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
    /// <summary>
    /// The 8.8 base speed a reflected missile re-ignites at: one pixel per update plus the 240/256 head start the
    /// reflection leaves.
    /// </summary>
    internal static readonly SignedEightEight ReflectedMissileIgnitionSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(496);

    /// <summary>
    /// What each of the ten firing directions adds to a projectile's 8.8 velocity on every update, in 256ths of a pixel
    /// per update per update. A beam (and a missile, before its own acceleration) gains sixteen on each axis it moves
    /// along; an ignited missile gains 64 along a cardinal direction and 54 on each axis of a diagonal; an ignited Super
    /// Missile gains 256 and 182. Both vertical directions of a side accelerate alike.
    /// </summary>
    internal static readonly FrozenDictionary<SamusProjectileDirection, SamusDirectionAccelerations> Accelerations =
        new Dictionary<SamusProjectileDirection, SamusDirectionAccelerations>
        {
            [SamusProjectileDirection.UpFacingRight] = new(new(0, -16), new(0, -64), new(0, -256)),
            [SamusProjectileDirection.UpRight] = new(new(16, -16), new(54, -54), new(182, -182)),
            [SamusProjectileDirection.Right] = new(new(16, 0), new(64, 0), new(256, 0)),
            [SamusProjectileDirection.DownRight] = new(new(16, 16), new(54, 54), new(182, 182)),
            [SamusProjectileDirection.DownFacingRight] = new(new(0, 16), new(0, 64), new(0, 256)),
            [SamusProjectileDirection.DownFacingLeft] = new(new(0, 16), new(0, 64), new(0, 256)),
            [SamusProjectileDirection.DownLeft] = new(new(-16, 16), new(-54, 54), new(-182, 182)),
            [SamusProjectileDirection.Left] = new(new(-16, 0), new(-64, 0), new(-256, 0)),
            [SamusProjectileDirection.UpLeft] = new(new(-16, -16), new(-54, -54), new(-182, -182)),
            [SamusProjectileDirection.UpFacingLeft] = new(new(0, -16), new(0, -64), new(0, -256)),
        }.ToFrozenDictionary();
}
