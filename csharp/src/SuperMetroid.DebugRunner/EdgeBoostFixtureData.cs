/// <summary>Named geometry and measured 16.16 witnesses for the original-CPU edge-boost fixture.</summary>
internal static class EdgeBoostFixtureData
{
    /// <summary>Top of the solid row-48 floor beneath the tested ledge.</summary>
    public const int FloorTop = 768;

    /// <summary>Expanded radius-19 center touching the underside at Y512.</summary>
    public const uint ExpandedUndersideY = 0x02130000;

    /// <summary>Down-aim expansion one frame too late at five pixels/frame.</summary>
    public const uint LateDownAimY = 0x02140000;

    /// <summary>Spin expansion one frame too late at five pixels/frame.</summary>
    public const uint LateSpinY = 0x02160000;

    /// <summary>Empty-space centers for zero/one/five-pixel first-frame motion.</summary>
    public static uint UnobstructedFirstFrameY(bool spin, int speedIndex) =>
        (spin, speedIndex) switch
        {
            (false, 0) => 0x020a0000,
            (false, 1) => 0x020b0000,
            (false, 2) => 0x020f0000,
            (true, 0) => 0x020c0000,
            (true, 1) => 0x020d0000,
            (true, 2) => 0x02110000,
            _ => throw new ArgumentOutOfRangeException(nameof(speedIndex)),
        };
}
