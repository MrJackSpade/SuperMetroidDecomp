namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$8F entry points used as setup programs by cartridge door headers.</summary>
internal static class DoorCodes
{
    /// <summary>
    /// <c>DoorCode_Scroll6_Green</c> at $8F:B981; changes room-scroll storage cell six
    /// to green.
    /// </summary>
    public const ushort DoorCode_Scroll6_Green = 0xb981;

    /// <summary>
    /// <c>DoorASM_Scroll_0_Green_1_Blue</c> at $8F:BE25; opens Construction Zone's
    /// two-screen vertical camera route after returning from First Missile.
    /// </summary>
    public const ushort DoorASM_Scroll_0_Green_1_Blue = 0xbe25;

    /// <summary>
    /// <c>DoorASM_ToCeresElevatorShaft</c> at $8F:E4E0; selects Mode 7 and installs
    /// the shaft's initial matrix and center.
    /// </summary>
    public const ushort DoorASM_ToCeresElevatorShaft = 0xe4e0;

    /// <summary>
    /// <c>DoorASM_FromCeresElevatorShaft</c> at $8F:E513; restores ordinary Mode 1
    /// rendering and disables Mode-7 IRQ/transfer handling.
    /// </summary>
    public const ushort DoorASM_FromCeresElevatorShaft = 0xe513;
}

/// <summary>Bank-$83 cartridge door-header pointers referenced by translated logic.</summary>
internal static class DoorPointers
{
    /// <summary>Early-route door that executes <see cref="DoorCodes.DoorCode_Scroll6_Green"/>.</summary>
    public const ushort EarlyRouteScrollSix = 0x8b3e;

    /// <summary>Door returning from First Missile to Construction Zone.</summary>
    public const ushort ConstructionZoneFromFirstMissile = 0x8fa6;

    /// <summary>Door leaving the Ceres elevator shaft for the falling-tile room.</summary>
    public const ushort FromCeresElevatorShaft = 0xab4c;
}
