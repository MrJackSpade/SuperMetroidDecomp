using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Where the cartridge keeps Mother Brain's fake-death room flash, for extraction and its ROM oracles only: the
/// simulation runs the flash by frame index and never locates it.
/// </summary>
[EncodingOwner("bank-$AD addresses of Mother Brain's fake-death room flash lists")]
internal static class MotherBrainRoomFlashSourceDefinitions
{
    /// <summary><c>MotherBrain_FakeDeath_RoomPalette_InstructionList</c> at $A9:D046: the room flash's fourteen timed entries.</summary>
    internal const ushort FlashList = 0xd046;

    /// <summary>Bytes in one timed room-flash entry: its duration word, then its palette operand word.</summary>
    internal const int FlashEntryBytes = 4;

    /// <summary>Byte offset of a timed room-flash entry's palette operand.</summary>
    internal const int FlashPaletteOperandOffset = 2;

    /// <summary>The final grey room palette at $A9:D082.</summary>
    internal const ushort FinalPalette = 0xd082;
}
