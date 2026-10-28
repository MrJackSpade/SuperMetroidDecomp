using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Verification's native view of the PLM pre-instruction identities (#1452): the bank-$84 routine word naming each
/// member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PlmPreInstructionNativeWords
{
    /// <summary>The bank-$84 routine word naming each <see cref="DownwardGatePreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<DownwardGatePreInstruction> DownwardGate = new(new Dictionary<DownwardGatePreInstruction, ushort>
    {
        [DownwardGatePreInstruction.None] = 0x0000,
        [DownwardGatePreInstruction.WakeIfTriggered] = 0xbb52,
        [DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow] = 0xbb6b,
        [DownwardGatePreInstruction.Inert] = 0xbb6a,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="NoobTubePlmPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<NoobTubePlmPreInstruction> NoobTube = new(new Dictionary<NoobTubePlmPreInstruction, ushort>
    {
        [NoobTubePlmPreInstruction.WakeOnAcceptedInput] = 0xd4bf,
        [NoobTubePlmPreInstruction.WakeOnPowerBomb] = 0xbd26,
        [NoobTubePlmPreInstruction.Inactive] = 0x86d0,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="SpeedBoosterEscapePreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<SpeedBoosterEscapePreInstruction> SpeedBoosterEscape = new(new Dictionary<SpeedBoosterEscapePreInstruction, ushort>
    {
        [SpeedBoosterEscapePreInstruction.None] = 0x0000,
        [SpeedBoosterEscapePreInstruction.WaitForSpeedBooster] = 0xb7ef,
        [SpeedBoosterEscapePreInstruction.WaitForSamusLeft] = 0xb82a,
        [SpeedBoosterEscapePreInstruction.AdvanceLava] = 0xb846,
    });
}
