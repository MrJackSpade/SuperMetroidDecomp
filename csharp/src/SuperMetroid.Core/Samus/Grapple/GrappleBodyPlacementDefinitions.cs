namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Native swing collision-body placement, independent of the displayed animation frame.</summary>
internal static class GrappleBodyPlacementDefinitions
{
    /// <summary>$9B:C2C2 GrappleSwingSamusXYOffsets_Left: 32 X/Y offsets relative to physical rope Start.</summary>
    private static readonly Offset16<RoomPixels>[] LeftOffsets =
    [
        new Offset16<RoomPixels>(0, -15), new Offset16<RoomPixels>(7, -14), new Offset16<RoomPixels>(12, -11), new Offset16<RoomPixels>(18, -10), new Offset16<RoomPixels>(22, -6), new Offset16<RoomPixels>(26, 2), new Offset16<RoomPixels>(29, 5), new Offset16<RoomPixels>(31, 10),
        new Offset16<RoomPixels>(31, 17), new Offset16<RoomPixels>(30, 22), new Offset16<RoomPixels>(27, 27), new Offset16<RoomPixels>(26, 31), new Offset16<RoomPixels>(21, 38), new Offset16<RoomPixels>(16, 41), new Offset16<RoomPixels>(16, 44), new Offset16<RoomPixels>(8, 46),
        new Offset16<RoomPixels>(0, 47), new Offset16<RoomPixels>(-7, 46), new Offset16<RoomPixels>(-12, 43), new Offset16<RoomPixels>(-18, 42), new Offset16<RoomPixels>(-22, 38), new Offset16<RoomPixels>(-26, 30), new Offset16<RoomPixels>(-29, 27), new Offset16<RoomPixels>(-31, 22),
        new Offset16<RoomPixels>(-31, 15), new Offset16<RoomPixels>(-30, 10), new Offset16<RoomPixels>(-27, 5), new Offset16<RoomPixels>(-26, 1), new Offset16<RoomPixels>(-21, -6), new Offset16<RoomPixels>(-16, -9), new Offset16<RoomPixels>(-16, -12), new Offset16<RoomPixels>(-8, -14),
    ];

    /// <summary>$9B:C302 GrappleSwingSamusXYOffsets_Right: 32 X/Y offsets relative to physical rope Start.</summary>
    private static readonly Offset16<RoomPixels>[] RightOffsets =
    [
        new Offset16<RoomPixels>(0, -14), new Offset16<RoomPixels>(9, -13), new Offset16<RoomPixels>(16, -10), new Offset16<RoomPixels>(18, -8), new Offset16<RoomPixels>(21, -5), new Offset16<RoomPixels>(26, 2), new Offset16<RoomPixels>(28, 6), new Offset16<RoomPixels>(30, 10),
        new Offset16<RoomPixels>(30, 16), new Offset16<RoomPixels>(30, 23), new Offset16<RoomPixels>(27, 28), new Offset16<RoomPixels>(26, 33), new Offset16<RoomPixels>(21, 37), new Offset16<RoomPixels>(16, 41), new Offset16<RoomPixels>(11, 43), new Offset16<RoomPixels>(5, 45),
        new Offset16<RoomPixels>(0, 46), new Offset16<RoomPixels>(-9, 45), new Offset16<RoomPixels>(-16, 42), new Offset16<RoomPixels>(-18, 40), new Offset16<RoomPixels>(-21, 37), new Offset16<RoomPixels>(-26, 30), new Offset16<RoomPixels>(-28, 26), new Offset16<RoomPixels>(-30, 22),
        new Offset16<RoomPixels>(-31, 15), new Offset16<RoomPixels>(-30, 9), new Offset16<RoomPixels>(-27, 4), new Offset16<RoomPixels>(-26, -1), new Offset16<RoomPixels>(-21, -5), new Offset16<RoomPixels>(-16, -9), new Offset16<RoomPixels>(-11, -11), new Offset16<RoomPixels>(-5, -13),
    ];

    /// <summary>$9B:C1C2 GrappleSwingSamusXYOffsets: native 256-entry selector is nearest eight-angle bucket, wrapping at 32.</summary>
    internal static Offset16<RoomPixels> Offset(ByteAngle mirroredAngle, bool left)
    {
        int pair = ((mirroredAngle.RawValue + 4) >> 3) & 31;
        return (left ? LeftOffsets : RightOffsets)[pair];
    }
}
