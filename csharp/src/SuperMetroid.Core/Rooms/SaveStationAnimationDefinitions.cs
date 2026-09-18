namespace SuperMetroid.Core.Rooms;

/// <summary>Compiled fixed mechanics definitions for the bank-$84 save-station animation.</summary>
internal static class SaveStationAnimationDefinitions
{
    /// <summary>
    /// Operand byte at <c>$84:AFF9</c> for
    /// <c>Instruction_PLM_TimerEqualsY_8Bit</c>. The pinned NTSC cartridge uses 21
    /// alternating electricity loops before displaying the saved-game message.
    /// </summary>
    public const ushort SaveAnimationLoops = 0x15;

    /// <summary>Native address of the compiled loop-count operand for parity verification.</summary>
    public const int NativeSaveAnimationLoopsAddress = 0x84aff9;
}
