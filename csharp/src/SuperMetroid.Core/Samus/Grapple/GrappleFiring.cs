using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Grapple.GrappleFiringDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The grapple's launch velocity, angle and hand origin for each firing direction.</summary>
internal static class GrappleFiring
{
    /// <summary>Returns exact launch components and angle for a firing direction.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every original value of the NTSC J/U v1.0 game
    /// and BeginFiring. Directions progress clockwise from up,
    /// facing right (0), through down, facing right (4), duplicated down (5), to up (9).
    /// Fixed cardinal/diagonal cases avoid runtime trigonometry. Quantization precedes
    /// scaling: 255 and 181 times twelve, not a rounded radius-3072 circle.
    /// The angle starts at a half turn for up and advances an eighth of a turn per octant clockwise; both downward
    /// directions share the down octant.
    /// </remarks>
    internal static GrappleLaunchDefinition Launch(SamusProjectileDirection direction)
    {
        int x = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight => DiagonalVelocity,
            SamusProjectileDirection.Right => CardinalVelocity,
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft => -DiagonalVelocity,
            SamusProjectileDirection.Left => -CardinalVelocity,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => 0,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {direction}."),
        };
        int y = direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft => -CardinalVelocity,
            SamusProjectileDirection.UpRight or SamusProjectileDirection.UpLeft => -DiagonalVelocity,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => DiagonalVelocity,
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => CardinalVelocity,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => 0,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {direction}."),
        };
        int octant = direction switch
        {
            SamusProjectileDirection.UpFacingRight => 0,
            SamusProjectileDirection.UpRight => 1,
            SamusProjectileDirection.Right => 2,
            SamusProjectileDirection.DownRight => 3,
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => 4,
            SamusProjectileDirection.DownLeft => 5,
            SamusProjectileDirection.Left => 6,
            SamusProjectileDirection.UpLeft => 7,
            SamusProjectileDirection.UpFacingLeft => 8,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {direction}."),
        };
        return new(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(x, y), RotationAngle.HalfTurn.TurnedBySteps(RotationAngle.StepsPerTurn / 8 * octant));
    }

    /// <summary>X and Y hand anchors selected by firing pose.
    /// Up/down facings remain distinct; down-left X is -4 versus down-right +3.
    /// Running raises only horizontal anchors by four pixels. All authored values
    /// independently match; subsequent graphics/pose corrections stay with callers.</summary>
    internal static Offset16<RoomPixels> Origin(SamusProjectileDirection aim, bool running)
    {
        int x = aim switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.Right => 2,
            SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight => 10,
            SamusProjectileDirection.DownFacingRight => 3,
            SamusProjectileDirection.DownFacingLeft => -4,
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft => -10,
            SamusProjectileDirection.Left or SamusProjectileDirection.UpFacingLeft => -2,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {aim}."),
        };
        int y = aim switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft => -16,
            SamusProjectileDirection.UpRight or SamusProjectileDirection.UpLeft => -12,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => running ? -2 : 2,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => 0,
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => 6,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {aim}."),
        };
        return Offset16<RoomPixels>.Of(x, y);
    }
}
