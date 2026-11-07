namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Compiled fixed mechanics definitions for the save-station animation.</summary>
internal static class SaveStationAnimationDefinitions
{
    /// <summary>
    /// Operand byte for
    /// <c>Instruction_PLM_TimerEqualsY_8Bit</c>. The pinned NTSC uses 21
    /// alternating electricity loops before displaying the saved-game message.
    /// </summary>
    public const int SaveAnimationLoops = 21;
}
