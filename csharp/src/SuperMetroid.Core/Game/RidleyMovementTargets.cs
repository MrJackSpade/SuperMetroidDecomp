namespace SuperMetroid.Core.Game;

/// <summary>Authored arena targets and health-dependent inertia selectors for Norfair Ridley.</summary>
public static class RidleyMovementTargets
{
    /// <summary>$A6:B60D, Ridley_Func_15 target X by facing (left / transition / right).</summary>
    public static ReadOnlySpan<ushort> DescendingPogoX => [192, 128, 64];
    /// <summary>$A6:B63B, Ridley_Func_16 target X by facing.</summary>
    public static ReadOnlySpan<ushort> AscendingPogoX => [64, 128, 192];
    /// <summary>$A6:B6C8, Ridley_Func_19 ground-attack side target X by facing.</summary>
    public static ReadOnlySpan<ushort> GroundAttackX => [176, 128, 96];
    /// <summary>$A6:BBEB, Ridley_Func_36 carry anchor X by facing; transition entry is zero.</summary>
    public static ReadOnlySpan<ushort> CarryAnchorX => [64, 0, 208];
    /// <summary>$A6:BC62, carry-release target X by facing; transition entry is zero.</summary>
    public static ReadOnlySpan<ushort> CarryReleaseX => [176, 0, 80];
    /// <summary>$A6:B439, health-stage movement divisor indexes for hover and pogo approach (not tail instructions).</summary>
    public static ReadOnlySpan<ushort> HoverDivisorIndexes => [4, 8, 10, 12];
    /// <summary>$A6:BB4E, separate health-stage movement divisor indexes for the grab approach.</summary>
    public static ReadOnlySpan<ushort> GrabDivisorIndexes => [1, 3, 7, 10];
}
