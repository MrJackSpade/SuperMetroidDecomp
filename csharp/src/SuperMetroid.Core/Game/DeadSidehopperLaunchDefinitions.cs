namespace SuperMetroid.Core.Game;

/// <summary>Four-phase signed 8.8 launch cycle for the Tourian sidehopper corpse.</summary>
public static class DeadSidehopperLaunchDefinitions
{
    /// <summary>$A9:D951, post-landing Y velocities, indexed by the modulo-four jump phase.</summary>
    public static ReadOnlySpan<ushort> Vertical => [0xfe00, 0xfe00, 0xfe00, 0xfc00];
    /// <summary>$A9:D959, post-landing X velocities paired with Vertical.</summary>
    public static ReadOnlySpan<ushort> Horizontal => [0x01c0, 0x0120, 0x0120, 0x0300];
}
