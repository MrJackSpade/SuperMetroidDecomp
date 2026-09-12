namespace SuperMetroid.Core.Game;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>$9B:C0DB GrappleBeamFireVelocityTable.X: signed extension velocity, scaled by $100.</summary>
    private static ReadOnlySpan<short> XVelocities => [0, 0x87c, 0xbf4, 0x87c, 0, 0, -0x87c, -0xbf4, -0x87c, 0];

    /// <summary>$9B:C0EF GrappleBeamFireVelocityTable.Y: signed extension velocity, scaled by $100.</summary>
    private static ReadOnlySpan<short> YVelocities => [-0xbf4, -0x87c, 0, 0x87c, 0xbf4, 0xbf4, 0x87c, 0, -0x87c, -0xbf4];

    /// <summary>$9B:C104 GrappleBeamFireAngles: initial native angle words for the ten firing directions.</summary>
    private static ReadOnlySpan<ushort> Angles => [0x8000, 0xa000, 0xc000, 0xe000, 0, 0, 0x2000, 0x4000, 0x6000, 0x8000];

    /// <summary>$9B:C122/$C172 GrappleBeamFireOffsets_NotRunning/Running_OriginX: identical physical hand offsets.</summary>
    private static ReadOnlySpan<short> OriginX => [2, 10, 2, 10, 3, -4, -10, -2, -10, -2];

    /// <summary>$9B:C136 GrappleBeamFireOffsets_NotRunning_OriginY: physical hand offsets before pose correction.</summary>
    private static ReadOnlySpan<short> DefaultOriginY => [-16, -12, 2, 0, 6, 6, 0, 2, -12, -16];

    /// <summary>$9B:C186 GrappleBeamFireOffsets_Running_OriginY: only horizontal directions differ from no-run.</summary>
    private static ReadOnlySpan<short> RunningOriginY => [-16, -12, -2, 0, 6, 6, 0, -2, -12, -16];

    internal static (short XVelocity, short YVelocity, ushort Angle) Launch(byte direction) =>
        (XVelocities[direction], YVelocities[direction], Angles[direction]);

    internal static bool TryGetOrigin(byte direction, bool running, out (short X, short Y) origin)
    {
        if (direction >= OriginX.Length)
        {
            origin = default;
            return false;
        }
        origin = (OriginX[direction], (running ? RunningOriginY : DefaultOriginY)[direction]);
        return true;
    }
}
