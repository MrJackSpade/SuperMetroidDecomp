namespace SuperMetroid.Core.Game;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>$9B:C0DB GrappleBeamFireVelocityTable.X, ten signed 8.8 velocity words.</summary>
    internal const int XVelocityReferenceAddress = 0x9bc0db;
    /// <summary>$9B:C0EF GrappleBeamFireVelocityTable.Y, ten signed 8.8 velocity words.</summary>
    internal const int YVelocityReferenceAddress = 0x9bc0ef;
    /// <summary>$9B:C104 GrappleBeamFireAngles, ten full-turn unsigned angle words.</summary>
    internal const int AngleReferenceAddress = 0x9bc104;
    /// <summary>$9B:C0DF cardinal component: twelve times the byte-sine maximum 255.</summary>
    private const short CardinalVelocity = 12 * 255;
    /// <summary>$9B:C0DD diagonal component: twelve times the byte-sine octant sample 181.</summary>
    private const short DiagonalVelocity = 12 * 181;
    /// <summary>$9B:C122/$C172 GrappleBeamFireOffsets_NotRunning/Running_OriginX: identical physical hand offsets.</summary>
    /// <remarks>#1165 retain: all words and both native copies independently checked.
    /// Pose-authored anchors are not circular samples: cardinal X is 2, diagonals 10,
    /// while downward-facing right/left use +3/-4. Mirroring requires a one-pixel
    /// exception; a guessed geometric generator obscures this short authored layout.</remarks>
    private static ReadOnlySpan<short> OriginX => [2, 10, 2, 10, 3, -4, -10, -2, -10, -2];

    /// <summary>$9B:C136 GrappleBeamFireOffsets_NotRunning_OriginY: physical hand offsets before pose correction.</summary>
    /// <remarks>#1165 retain after independent full-word/native-consumer review.
    /// Values are mirrored by facing but use irregular pose-specific vertical anchors,
    /// not uniform radial or linear spacing. A half-table plus reflection would save
    /// little and hide the direct direction-to-hand relationship.</remarks>
    private static ReadOnlySpan<short> DefaultOriginY => [-16, -12, 2, 0, 6, 6, 0, 2, -12, -16];

    /// <summary>$9B:C186 GrappleBeamFireOffsets_Running_OriginY: only horizontal directions differ from no-run.</summary>
    /// <remarks>#1165 retain: independently checked signed words and running-only
    /// selection. Horizontal origins change from +2 to -2; remaining authored anchors
    /// match the no-run set. Keep this separate movement policy directly readable.</remarks>
    private static ReadOnlySpan<short> RunningOriginY => [-16, -12, -2, 0, 6, 6, 0, -2, -12, -16];

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

    internal static (short X, short Y) Origin(byte direction, bool running)
    {
        if (direction >= OriginX.Length)
        {
            throw new InvalidDataException(
                $"Grapple firing direction {direction} is outside the ten compiled origin records.");
        }
        return (OriginX[direction], (running ? RunningOriginY : DefaultOriginY)[direction]);
    }
}
