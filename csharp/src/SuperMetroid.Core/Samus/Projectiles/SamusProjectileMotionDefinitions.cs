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
    /// The base speed a reflected missile re-ignites at: one pixel per update plus the head start the reflection
    /// leaves, a little under a pixel.
    /// </summary>
    internal static readonly SignedEightEight ReflectedMissileIgnitionSpeed = SignedEightEight.FromPixelFraction(31, 16);

    /// <summary>
    /// What each of the ten firing directions adds to a projectile's velocity on every update: a beam (and a missile,
    /// before its own acceleration) gains a little on each axis it moves along, an ignited missile more, and an ignited
    /// Super Missile the most, a diagonal gaining a little less on each axis than a cardinal direction. Both vertical
    /// directions of a side accelerate alike.
    /// </summary>
    internal static readonly IdentityTable<SamusProjectileDirection, SamusDirectionAccelerations> Accelerations = new(new Dictionary<SamusProjectileDirection, SamusDirectionAccelerations>
        {
            [SamusProjectileDirection.UpFacingRight] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 4)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(-1))),
            [SamusProjectileDirection.UpRight] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(-1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(27, 128), SignedEightEight.FromPixelFraction(-27, 128)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(91, 128), SignedEightEight.FromPixelFraction(-91, 128))),
            [SamusProjectileDirection.Right] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.Zero),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(1, 4), SignedEightEight.Zero),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixels(1), SignedEightEight.Zero)),
            [SamusProjectileDirection.DownRight] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(27, 128), SignedEightEight.FromPixelFraction(27, 128)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(91, 128), SignedEightEight.FromPixelFraction(91, 128))),
            [SamusProjectileDirection.DownFacingRight] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 4)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(1))),
            [SamusProjectileDirection.DownFacingLeft] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(1, 4)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(1))),
            [SamusProjectileDirection.DownLeft] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 16), SignedEightEight.FromPixelFraction(1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-27, 128), SignedEightEight.FromPixelFraction(27, 128)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-91, 128), SignedEightEight.FromPixelFraction(91, 128))),
            [SamusProjectileDirection.Left] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 16), SignedEightEight.Zero),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 4), SignedEightEight.Zero),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixels(-1), SignedEightEight.Zero)),
            [SamusProjectileDirection.UpLeft] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 16), SignedEightEight.FromPixelFraction(-1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-27, 128), SignedEightEight.FromPixelFraction(-27, 128)),
                new PixelVelocity<RoomPixels>(SignedEightEight.FromPixelFraction(-91, 128), SignedEightEight.FromPixelFraction(-91, 128))),
            [SamusProjectileDirection.UpFacingLeft] = new(
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 16)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-1, 4)),
                new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixels(-1))),
        });
}
