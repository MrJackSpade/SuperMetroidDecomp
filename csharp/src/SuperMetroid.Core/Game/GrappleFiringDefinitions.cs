namespace SuperMetroid.Core.Game;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>$9B:C0DB GrappleBeamFireVelocityTable.X: signed extension velocity, scaled by $100.</summary>
    /// <remarks>
    /// #625 exact candidate: for direction d in 0..9, let q = d &lt;= 4 ? d : d - 1.
    /// X = 12 * B(32*q), where B(a) is the signed periodic byte sine: reduce a modulo 256,
    /// take min(255, floor(256*sin((a mod 128)*pi/128))), and negate in the second half-cycle.
    /// Quantization BEFORE scaling is essential: the cardinal magnitude is 12*255 = 3060,
    /// and the diagonal is 12*181 = 2172; rounding a full-precision radius-3072 circle is not equivalent.
    /// This reuses the independently reproduced enemy byte-sine generator, with no entry corrections.
    /// LookupTableResearch verifies all 30 launch words against compiled definitions, NTSC J/U v1.0 ROM,
    /// and pinned disassembly. Generator compatibility is proven; historical tooling is not established.
    /// Runtime replacement and its performance assessment are deferred.
    /// </remarks>
    private static ReadOnlySpan<short> XVelocities => [0, 0x87c, 0xbf4, 0x87c, 0, 0, -0x87c, -0xbf4, -0x87c, 0];

    /// <summary>$9B:C0EF GrappleBeamFireVelocityTable.Y: signed extension velocity, scaled by $100.</summary>
    /// <remarks>#625: Y = 12 * B(32*q - 64), using the signed, byte-quantized sine and direction mapping
    /// documented on XVelocities. All ten values match exactly, including both downward-facing directions.</remarks>
    private static ReadOnlySpan<short> YVelocities => [-0xbf4, -0x87c, 0, 0x87c, 0xbf4, 0xbf4, 0x87c, 0, -0x87c, -0xbf4];

    /// <summary>$9B:C104 GrappleBeamFireAngles: initial native angle words for the ten firing directions.</summary>
    /// <remarks>#625: for q = d &lt;= 4 ? d : d - 1, angle = (0x8000 + 0x2000*q) modulo 65536.
    /// Verified for every d in 0..9 against ROM, disassembly, and the compiled table.</remarks>
    private static ReadOnlySpan<ushort> Angles => [0x8000, 0xa000, 0xc000, 0xe000, 0, 0, 0x2000, 0x4000, 0x6000, 0x8000];

    /// <summary>$9B:C122/$C172 GrappleBeamFireOffsets_NotRunning/Running_OriginX: identical physical hand offsets.</summary>
    private static ReadOnlySpan<short> OriginX => [2, 10, 2, 10, 3, -4, -10, -2, -10, -2];

    /// <summary>$9B:C136 GrappleBeamFireOffsets_NotRunning_OriginY: physical hand offsets before pose correction.</summary>
    private static ReadOnlySpan<short> DefaultOriginY => [-16, -12, 2, 0, 6, 6, 0, 2, -12, -16];

    /// <summary>$9B:C186 GrappleBeamFireOffsets_Running_OriginY: only horizontal directions differ from no-run.</summary>
    private static ReadOnlySpan<short> RunningOriginY => [-16, -12, -2, 0, 6, 6, 0, -2, -12, -16];

    internal static (short XVelocity, short YVelocity, ushort Angle) Launch(byte direction) =>
        (XVelocities[direction], YVelocities[direction], Angles[direction]);

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
