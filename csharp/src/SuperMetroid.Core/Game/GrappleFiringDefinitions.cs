namespace SuperMetroid.Core.Game;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>$9B:C0DF cardinal component: twelve times the byte-sine maximum 255.</summary>
    private const short CardinalVelocity = 12 * 255;
    /// <summary>$9B:C0DD diagonal component: twelve times the byte-sine octant sample 181.</summary>
    private const short DiagonalVelocity = 12 * 181;
    /// <summary>Returns exact launch components and angle for firing direction 0..9.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC J/U v1.0 original word,
    /// pinned bank_9B.asm and BeginFiring. Directions progress clockwise from up,
    /// facing right (0), through down, facing right (4), duplicated down (5), to up (9).
    /// Fixed cardinal/diagonal cases avoid runtime trigonometry. Quantization precedes
    /// scaling: 255 and 181 times twelve, not a rounded radius-3072 circle.
    /// Angle is (0x8000+0x2000*q) modulo 65536, q=d for d&lt;=4 else d-1.
    /// Preserve the former span's IndexOutOfRangeException for unsupported directions.
    /// </remarks>
    internal static (short XVelocity, short YVelocity, ushort Angle) Launch(byte direction)
    {
        if (direction >= 10) throw new IndexOutOfRangeException();
        short x = direction switch
        {
            1 or 3 => DiagonalVelocity,
            2 => CardinalVelocity,
            6 or 8 => -DiagonalVelocity,
            7 => -CardinalVelocity,
            _ => 0,
        };
        short y = direction switch
        {
            0 or 9 => -CardinalVelocity,
            1 or 8 => -DiagonalVelocity,
            3 or 6 => DiagonalVelocity,
            4 or 5 => CardinalVelocity,
            _ => 0,
        };
        int octant = direction <= 4 ? direction : direction - 1;
        return (x, y, unchecked((ushort)(0x8000 + 0x2000 * octant)));
    }

    /// <summary>$9B:C122/$C172 X and $C136/$C186 Y hand anchors selected by firing pose.
    /// Up/down facings remain distinct; down-left X is -4 versus down-right +3.
    /// Running raises only horizontal anchors by four pixels. All native words
    /// independently match; subsequent graphics/pose corrections stay with callers.</summary>
    internal static (short X, short Y) Origin(byte direction, bool running)
    {
        if (direction >= 10)
        {
            throw new InvalidDataException(
                $"Grapple firing direction {direction} is outside the ten compiled origin records.");
        }
        var aim = (GrappleOriginDirection)direction;
        short x = aim switch
        {
            GrappleOriginDirection.UpFacingRight or GrappleOriginDirection.Right => 2,
            GrappleOriginDirection.UpRight or GrappleOriginDirection.DownRight => 10,
            GrappleOriginDirection.DownFacingRight => 3,
            GrappleOriginDirection.DownFacingLeft => -4,
            GrappleOriginDirection.DownLeft or GrappleOriginDirection.UpLeft => -10,
            _ => -2,
        };
        short y = aim switch
        {
            GrappleOriginDirection.UpFacingRight or GrappleOriginDirection.UpFacingLeft => -16,
            GrappleOriginDirection.UpRight or GrappleOriginDirection.UpLeft => -12,
            GrappleOriginDirection.Right or GrappleOriginDirection.Left => running ? (short)-2 : (short)2,
            GrappleOriginDirection.DownRight or GrappleOriginDirection.DownLeft => 0,
            _ => 6,
        };
        return (x, y);
    }
}

/// <summary>$9B:C122-$C198 origin-field indices, clockwise from up facing right
/// to up facing left, with distinct downward-facing hand poses at indices 4 and 5.</summary>
internal enum GrappleOriginDirection : byte
{
    /// <summary>Index zero selects the native upward hand anchor for Samus facing right.</summary>
    UpFacingRight = 0,
    /// <summary>Index one selects the first diagonal-up firing pose's hand anchor.</summary>
    UpRight = 1,
    /// <summary>Index two selects the horizontal firing pose's hand anchor.</summary>
    Right = 2,
    /// <summary>Index three selects the diagonal-down firing pose before the distinct downward-facing record.</summary>
    DownRight = 3,
    /// <summary>Index four preserves the right-facing downward pose's separate native hand anchor.</summary>
    DownFacingRight = 4,
    /// <summary>Index five preserves the left-facing downward pose's separate native hand anchor.</summary>
    DownFacingLeft = 5,
    /// <summary>Index six selects the diagonal-down firing pose with the opposite horizontal sign.</summary>
    DownLeft = 6,
    /// <summary>Index seven selects the horizontal firing pose with the opposite horizontal sign.</summary>
    Left = 7,
    /// <summary>Index eight selects the diagonal-up firing pose with the opposite horizontal sign.</summary>
    UpLeft = 8,
    /// <summary>Index nine selects the native upward hand anchor for Samus facing left.</summary>
    UpFacingLeft = 9,
}
