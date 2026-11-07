using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Verification's native view of the PLM pre-instruction identities (#1452): the bank-$84 routine word naming each
/// member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class RoomObjectFrameBehaviorNativeWords
{
    /// <summary>The bank-$84 routine word naming each <see cref="DownwardGateFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<DownwardGateFrameBehavior> DownwardGate = new(new Dictionary<DownwardGateFrameBehavior, ushort>
    {
        [DownwardGateFrameBehavior.None] = 0x0000,
        [DownwardGateFrameBehavior.WakeIfTriggered] = 0xbb52,
        [DownwardGateFrameBehavior.WakeIfTriggeredOrSamusBelow] = 0xbb6b,
        [DownwardGateFrameBehavior.Inert] = 0xbb6a,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="NoobTubeObjectFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<NoobTubeObjectFrameBehavior> NoobTube = new(new Dictionary<NoobTubeObjectFrameBehavior, ushort>
    {
        [NoobTubeObjectFrameBehavior.WakeOnAcceptedInput] = 0xd4bf,
        [NoobTubeObjectFrameBehavior.WakeOnPowerBomb] = 0xbd26,
        [NoobTubeObjectFrameBehavior.Inactive] = 0x86d0,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="SpeedBoosterEscapeFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<SpeedBoosterEscapeFrameBehavior> SpeedBoosterEscape = new(new Dictionary<SpeedBoosterEscapeFrameBehavior, ushort>
    {
        [SpeedBoosterEscapeFrameBehavior.None] = 0x0000,
        [SpeedBoosterEscapeFrameBehavior.WaitForSpeedBooster] = 0xb7ef,
        [SpeedBoosterEscapeFrameBehavior.WaitForSamusLeft] = 0xb82a,
        [SpeedBoosterEscapeFrameBehavior.AdvanceLava] = 0xb846,
    });
}
