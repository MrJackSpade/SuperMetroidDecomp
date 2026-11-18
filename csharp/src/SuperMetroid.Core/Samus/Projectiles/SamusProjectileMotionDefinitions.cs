using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>
    /// The horizontal and vertical speed every beam combination launches at, in 256ths of a pixel per update:
    /// four pixels.
    /// </summary>
    internal static readonly SignedEightEight CardinalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(1024);

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
    /// <summary>
    /// The most extension links a shot's block reaction follows to the block it reacts to; a chain that does not end
    /// within them is malformed room data.
    /// </summary>
    internal const int LongestExtensionChain = 16;
    /// <summary>The diagonal speed every beam combination launches at, in 256ths of a pixel per update.</summary>
    internal static readonly SignedEightEight DiagonalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(683);
    /// <summary>The base speed a fired missile ignites at: one pixel per update.</summary>
    internal static readonly SignedEightEight MissileIgnitionSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
    /// <summary>
    /// The base speed a reflected missile re-ignites at: one pixel per update plus the 240/256 head start the
    /// reflection leaves.
    /// </summary>
    internal static readonly SignedEightEight ReflectedMissileIgnitionSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(496);

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
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -64),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -256)),
            [SamusProjectileDirection.UpRight] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(16, -16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(54, -54),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(182, -182)),
            [SamusProjectileDirection.Right] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(16, 0),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(64, 0),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(256, 0)),
            [SamusProjectileDirection.DownRight] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(16, 16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(54, 54),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(182, 182)),
            [SamusProjectileDirection.DownFacingRight] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 64),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 256)),
            [SamusProjectileDirection.DownFacingLeft] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 64),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 256)),
            [SamusProjectileDirection.DownLeft] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-16, 16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-54, 54),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-182, 182)),
            [SamusProjectileDirection.Left] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-16, 0),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-64, 0),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-256, 0)),
            [SamusProjectileDirection.UpLeft] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-16, -16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-54, -54),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-182, -182)),
            [SamusProjectileDirection.UpFacingLeft] = new(
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -16),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -64),
                EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -256)),
        }.ToFrozenDictionary();
}
