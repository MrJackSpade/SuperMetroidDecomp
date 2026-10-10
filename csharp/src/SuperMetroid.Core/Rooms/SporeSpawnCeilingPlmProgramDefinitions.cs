namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The two bounded bank-$84 Spore Spawn ceiling instruction lists. The
/// crumble list intentionally falls through into the clear list after its
/// three timed frames; adjacent Botwoon setup code is not instruction data.
/// </summary>
internal static class SporeSpawnCeilingPlmProgramDefinitions
{
    /// <summary><c>$84:AB12</c>: sound followed by three four-frame crumble draws.</summary>
    internal const ushort Crumble = RoomPlmInstructionLists.CrumbleSporeSpawnCeiling;
    /// <summary><c>$84:AB21</c>: four-frame clear followed by deletion.</summary>
    internal const ushort Clear = RoomPlmInstructionLists.ClearSporeSpawnCeiling;
    /// <summary><c>$84:AB14</c>: library-two block-crumble sound <c>$0A</c>.</summary>
    internal const byte CrumbleSoundId = 0x0a;
    /// <summary>Native duration of each ceiling appearance.</summary>
    internal const ushort FrameDuration = 4;

    /// <summary>Resolves a word from either compiled ceiling instruction list, including timed draw operands.</summary>
    /// <param name="address">Bank-$84 address of the instruction word or timed-frame operand.</param>
    /// <param name="value">Compiled word value when recognized, or zero when the address is outside these lists.</param>
    /// <returns><see langword="true"/> when the address belongs to a supported word in the crumble or clear list.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address >= Crumble + 3 && address < Clear)
        {
            int offset = address - Crumble - 3;
            if (offset % 4 == 0)
            {
                value = FrameDuration;
                return true;
            }
            if (offset % 4 == 2)
            {
                value = SporeSpawnCeilingPlmDrawDefinitions.CrumbleFramePointer(
                    offset / 4);
                return true;
            }
        }
        value = address switch
        {
            Crumble => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
            Clear => FrameDuration,
            Clear + 2 => SporeSpawnCeilingPlmDrawDefinitions.ClearPointer,
            Clear + 4 => RoomPlmInstructionCodes.Delete,
            _ => 0,
        };
        return address is Crumble or Clear or Clear + 2 or Clear + 4;
    }

    /// <summary>Resolves the crumble list's packed library-two sound operand.</summary>
    /// <param name="address">Bank-$84 byte address to check; the sound operand is at <c>$84:AB14</c>.</param>
    /// <param name="value">Crumble sound ID on success, or zero when the address is not the packed operand.</param>
    /// <returns><see langword="true"/> only for the sound byte embedded between the crumble instruction words.</returns>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == Crumble + 2)
        {
            value = CrumbleSoundId;
            return true;
        }
        value = 0;
        return false;
    }
}
