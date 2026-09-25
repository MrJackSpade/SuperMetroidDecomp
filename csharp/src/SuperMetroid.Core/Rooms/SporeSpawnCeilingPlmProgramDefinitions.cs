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
    /// <summary><c>$84:AB27</c>: first byte of adjacent Botwoon setup code, not Spore Spawn data.</summary>
    internal const ushort EndExclusive = 0xab27;
    /// <summary><c>$84:AB14</c>: library-two block-crumble sound <c>$0A</c>.</summary>
    internal const byte CrumbleSoundId = 0x0a;
    /// <summary>Native duration of each ceiling appearance.</summary>
    internal const ushort FrameDuration = 4;

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

    internal static IEnumerable<ushort> NativeWordAddresses()
    {
        yield return Crumble;
        for (int frame = 0; frame < 3; frame++)
        {
            yield return checked((ushort)(Crumble + 3 + frame * 4));
            yield return checked((ushort)(Crumble + 5 + frame * 4));
        }
        yield return Clear;
        yield return checked((ushort)(Clear + 2));
        yield return checked((ushort)(Clear + 4));
    }
}
