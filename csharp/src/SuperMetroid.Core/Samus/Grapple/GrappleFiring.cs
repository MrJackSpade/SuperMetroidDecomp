using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Grapple.GrappleFiringDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The grapple's launch velocity, angle and hand origin for each firing direction.</summary>
internal static class GrappleFiring
{
    /// <summary>Returns exact launch components and angle for a firing direction.</summary>
    /// <remarks>
    /// Directions progress clockwise from up,
    /// facing right (0), through down, facing right (4), duplicated down (5), to up (9).
    /// Fixed cardinal/diagonal cases avoid runtime trigonometry. Quantization precedes
    /// scaling: 255 and 181 times twelve, not a rounded radius-3072 circle.
    /// The angle is a half turn for up and turns on an eighth of a turn for each eighth of the compass clockwise; both
    /// upward and both downward directions share their angle.
    /// </remarks>
    internal static GrappleLaunchDefinition Launch(SamusProjectileDirection direction)
    {
        SignedEightEight x = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight => DiagonalSpeed,
            SamusProjectileDirection.Right => CardinalSpeed,
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft => -DiagonalSpeed,
            SamusProjectileDirection.Left => -CardinalSpeed,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => SignedEightEight.Zero,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {direction}."),
        };
        SignedEightEight y = direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft => -CardinalSpeed,
            SamusProjectileDirection.UpRight or SamusProjectileDirection.UpLeft => -DiagonalSpeed,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => DiagonalSpeed,
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => CardinalSpeed,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => SignedEightEight.Zero,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {direction}."),
        };
        RotationAngle angle = direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft => RotationAngle.HalfTurn,
            SamusProjectileDirection.UpRight => RotationAngle.HalfTurn + RotationAngle.EighthTurn,
            SamusProjectileDirection.Right => RotationAngle.ThreeQuarterTurn,
            SamusProjectileDirection.DownRight => RotationAngle.ThreeQuarterTurn + RotationAngle.EighthTurn,
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => RotationAngle.Zero,
            SamusProjectileDirection.DownLeft => RotationAngle.EighthTurn,
            SamusProjectileDirection.Left => RotationAngle.QuarterTurn,
            SamusProjectileDirection.UpLeft => RotationAngle.QuarterTurn + RotationAngle.EighthTurn,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {direction}."),
        };
        return new(new EightEightVector<RoomPixels>(x, y), angle);
    }

    /// <summary>X and Y hand anchors selected by firing pose.
    /// Up/down facings remain distinct; down-left X is -4 versus down-right +3.
    /// Running raises only horizontal anchors by four pixels. Every authored value matches;
    /// later graphics and pose corrections belong to the firing.</summary>
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
