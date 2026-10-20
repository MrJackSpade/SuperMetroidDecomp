namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Native swing collision-body placement, independent of the displayed animation frame.</summary>
internal static class GrappleBodyPlacementDefinitions
{
    /// <summary>$9B:C2C2 GrappleSwingSamusXYOffsets_Left: 32 X/Y offsets relative to physical rope Start.</summary>
    private static readonly (sbyte X, sbyte Y)[] LeftOffsets =
    [
        (0, -15), (7, -14), (12, -11), (18, -10), (22, -6), (26, 2), (29, 5), (31, 10),
        (31, 17), (30, 22), (27, 27), (26, 31), (21, 38), (16, 41), (16, 44), (8, 46),
        (0, 47), (-7, 46), (-12, 43), (-18, 42), (-22, 38), (-26, 30), (-29, 27), (-31, 22),
        (-31, 15), (-30, 10), (-27, 5), (-26, 1), (-21, -6), (-16, -9), (-16, -12), (-8, -14),
    ];

    /// <summary>$9B:C302 GrappleSwingSamusXYOffsets_Right: 32 X/Y offsets relative to physical rope Start.</summary>
    private static readonly (sbyte X, sbyte Y)[] RightOffsets =
    [
        (0, -14), (9, -13), (16, -10), (18, -8), (21, -5), (26, 2), (28, 6), (30, 10),
        (30, 16), (30, 23), (27, 28), (26, 33), (21, 37), (16, 41), (11, 43), (5, 45),
        (0, 46), (-9, 45), (-16, 42), (-18, 40), (-21, 37), (-26, 30), (-28, 26), (-30, 22),
        (-31, 15), (-30, 9), (-27, 4), (-26, -1), (-21, -5), (-16, -9), (-11, -11), (-5, -13),
    ];

    /// <summary>$9B:C1C2 GrappleSwingSamusXYOffsets: native 256-entry selector is nearest eight-angle bucket, wrapping at 32.</summary>
    internal static (sbyte X, sbyte Y) Offset(SnesByteAngle mirroredAngle, bool left)
    {
        int pair = ((mirroredAngle.RawValue + 4) >> 3) & 31;
        return (left ? LeftOffsets : RightOffsets)[pair];
    }
}
