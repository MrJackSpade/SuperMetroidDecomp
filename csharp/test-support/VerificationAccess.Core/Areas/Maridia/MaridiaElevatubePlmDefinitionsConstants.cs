using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>Cartridge values and helpers of <see cref="MaridiaElevatubePlmDefinitions"/> that only verification reads.</summary>
internal static class MaridiaElevatubePlmDefinitionsConstants
{
    /// <summary><c>$84:B8F0</c>: sixteen-frame delay and one-block draw.</summary>
    internal const ushort InstructionList = RoomPlmInstructionListsConstants.MaridiaElevatube;
    /// <summary><c>$84:B8F6</c>: library-two elevatube sound operand.</summary>
    internal const byte SoundId = 0x15;
    /// <summary>Supplies the delay, draw pointer, sound command, and deletion word at their native instruction addresses.</summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = address switch
        {
            InstructionList => 16,
            InstructionList + 2 => PlmDrawAddresses.MaridiaElevatubeBlock,
            InstructionList + 4 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            InstructionList + 7 => (ushort)RoomPlmInstruction.Delete,
            _ => 0,
        };
        return address is InstructionList or InstructionList + 2 or
            InstructionList + 4 or InstructionList + 7;
    }
    /// <summary>Supplies the elevatube sound operand at the byte following its library-two command.</summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == InstructionList + 6)
        {
            value = SoundId;
            return true;
        }
        value = 0;
        return false;
    }
}
