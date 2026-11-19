using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>
    /// The horizontal and vertical speed every beam combination launches at: four pixels an update.
    /// </summary>
    internal static readonly SignedEightEight CardinalSpeed = SignedEightEight.FromPixels(4);

    /// <summary>The most rows below its top row a Wave shot's leading edge may reach and still scan terrain: one reaching further scans none.</summary>
    internal const int WaveScanLongestReach = 15;
    /// <summary>
    /// How far, in pixels, a beam or missile may fly above or left of the view before its flight deletes it: the
    /// movement window's apron on those sides.
    /// </summary>
    internal const int MovementWindowApron = 64;
    /// <summary>
    /// How far, in pixels, right of the view's left edge or below its top edge a beam or missile's flight deletes it:
    /// the movement window reaches up to the pixel before.
    /// </summary>
    internal const int MovementWindowFarEdge = 320;
    /// <summary>The diagonal speed every beam combination launches at, each update.</summary>
    internal static readonly SignedEightEight DiagonalSpeed = SignedEightEight.FromPixelFraction(683, 256);
    /// <summary>The base speed a fired missile ignites at: one pixel per update.</summary>
    internal static readonly SignedEightEight MissileIgnitionSpeed = SignedEightEight.FromPixels(1);
    /// <summary>
    /// The base speed a reflected missile re-ignites at: one pixel per update plus the 240/256 head start the
    /// reflection leaves.
    /// </summary>
    internal static readonly SignedEightEight ReflectedMissileIgnitionSpeed = SignedEightEight.FromPixelFraction(31, 16);

    /// <summary>
    /// What each of the ten firing directions adds to a projectile's velocity on every update, in 256ths of a pixel
    /// per update per update. A beam (and a missile, before its own acceleration) gains sixteen on each axis it moves
    /// along; an ignited missile gains 64 along a cardinal direction and 54 on each axis of a diagonal; an ignited Super
    /// Missile gains 256 and 182. Both vertical directions of a side accelerate alike.
    /// </summary>
    internal static readonly FrozenDictionary<SamusProjectileDirection, SamusDirectionAccelerations> Accelerations =
        new Dictionary<SamusProjectileDirection, SamusDirectionAccelerations>
        {
            [SamusProjectileDirection.UpFacingRight] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 4)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(-1))),
            [SamusProjectileDirection.UpRight] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(-1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(27, 128), SignedEightEight.FromPixelFraction(-27, 128)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(91, 128), SignedEightEight.FromPixelFraction(-91, 128))),
            [SamusProjectileDirection.Right] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.Zero),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(1, 4), SignedEightEight.Zero),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixels(1), SignedEightEight.Zero)),
            [SamusProjectileDirection.DownRight] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(27, 128), SignedEightEight.FromPixelFraction(27, 128)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(91, 128), SignedEightEight.FromPixelFraction(91, 128))),
            [SamusProjectileDirection.DownFacingRight] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 4)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(1))),
            [SamusProjectileDirection.DownFacingLeft] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 4)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(1))),
            [SamusProjectileDirection.DownLeft] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 16), SignedEightEight.FromPixelFraction(1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-27, 128), SignedEightEight.FromPixelFraction(27, 128)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-91, 128), SignedEightEight.FromPixelFraction(91, 128))),
            [SamusProjectileDirection.Left] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 16), SignedEightEight.Zero),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 4), SignedEightEight.Zero),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixels(-1), SignedEightEight.Zero)),
            [SamusProjectileDirection.UpLeft] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 16), SignedEightEight.FromPixelFraction(-1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-27, 128), SignedEightEight.FromPixelFraction(-27, 128)),
                new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-91, 128), SignedEightEight.FromPixelFraction(-91, 128))),
            [SamusProjectileDirection.UpFacingLeft] = new(
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 16)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 4)),
                new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(-1))),
        }.ToFrozenDictionary();
}
