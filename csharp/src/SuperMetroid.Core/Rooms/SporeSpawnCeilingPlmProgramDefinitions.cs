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

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address is >= (Crumble + 3) and < Clear)
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
        // Word positions are byte offsets from Crumble; the clear list starts at Clear.
        ushort? word = (address - Crumble) switch
        {
            0 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            Clear - Crumble => FrameDuration,
            Clear + 2 - Crumble => (ushort)SporeSpawnCeilingDraw.Clear,
            Clear + 4 - Crumble => (ushort)RoomPlmInstruction.Delete,
            _ => null,
        };
        value = word ?? 0;
        return word.HasValue;
    }

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
