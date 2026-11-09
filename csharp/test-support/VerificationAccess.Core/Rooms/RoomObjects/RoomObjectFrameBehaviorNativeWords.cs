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

    /// <summary>The bank-$84 routine word of each wait of the glass tube (<see cref="NoobTubePhase"/>).</summary>
    internal static readonly NativeWordIdentities<NoobTubePhase> NoobTube = new(new Dictionary<NoobTubePhase, ushort>
    {
        [NoobTubePhase.WaitingForInput] = 0xd4bf,
        [NoobTubePhase.WaitingForPowerBomb] = 0xbd26,
    });

    /// <summary>The bank-$84 routine word of each wait of the lavaquake (<see cref="SpeedBoosterEscapePhase"/>).</summary>
    internal static readonly NativeWordIdentities<SpeedBoosterEscapePhase> SpeedBoosterEscape = new(new Dictionary<SpeedBoosterEscapePhase, ushort>
    {
        [SpeedBoosterEscapePhase.WaitingForSpeedBooster] = 0xb7ef,
        [SpeedBoosterEscapePhase.WaitingForSamusLeft] = 0xb82a,
        [SpeedBoosterEscapePhase.AdvancingLava] = 0xb846,
    });
}
