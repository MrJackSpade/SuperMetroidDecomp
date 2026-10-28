using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Grapple.GrappleFiringDefinitions;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The grapple's launch velocity, angle and hand origin for each firing direction.</summary>
internal static class GrappleFiring
{
    /// <summary>Returns exact launch components and angle for firing direction 0..9.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC J/U v1.0 original word,
    /// pinned bank_9B.asm and BeginFiring. Directions progress clockwise from up,
    /// facing right (0), through down, facing right (4), duplicated down (5), to up (9).
    /// Fixed cardinal/diagonal cases avoid runtime trigonometry. Quantization precedes
    /// scaling: 255 and 181 times twelve, not a rounded radius-3072 circle.
    /// Angle is (0x8000+0x2000*q) modulo 65536, q=d for d&lt;=4 else d-1.
    /// throw ArgumentOutOfRangeException for unsupported directions.
    /// </remarks>
    internal static GrappleLaunchDefinition Launch(int direction)
    {
        if ((uint)direction >= 10) throw new ArgumentOutOfRangeException(nameof(direction));
        int x = direction switch
        {
            1 or 3 => DiagonalVelocity,
            2 => CardinalVelocity,
            6 or 8 => -DiagonalVelocity,
            7 => -CardinalVelocity,
            _ => 0,
        };
        int y = direction switch
        {
            0 or 9 => -CardinalVelocity,
            1 or 8 => -DiagonalVelocity,
            3 or 6 => DiagonalVelocity,
            4 or 5 => CardinalVelocity,
            _ => 0,
        };
        int octant = direction <= 4 ? direction : direction - 1;
        return new(new EightEightVector<RoomPixels>(x, y), RotationAngle.FromRaw(unchecked((ushort)(RotationAngle.HalfTurn.RawValue + RotationAngle.QuarterTurn.RawValue / 2 * octant))));
    }

    /// <summary>$9B:C122/$C172 X and $C136/$C186 Y hand anchors selected by firing pose.
    /// Up/down facings remain distinct; down-left X is -4 versus down-right +3.
    /// Running raises only horizontal anchors by four pixels. All native words
    /// independently match; subsequent graphics/pose corrections stay with callers.</summary>
    internal static Offset16<RoomPixels> Origin(int direction, bool running)
    {
        if ((uint)direction >= 10)
        {
            throw new InvalidDataException(
                $"Grapple firing direction {direction} is outside the ten compiled origin records.");
        }
        SamusProjectileDirection aim = SamusProjectileDirectionCodec.FromAimIndex(direction);
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
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => running ? (short)-2 : (short)2,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => 0,
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => 6,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileDirection)} {aim}."),
        };
        return Offset16<RoomPixels>.Of(x, y);
    }
}
