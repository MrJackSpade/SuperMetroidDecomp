namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Mother Brain fake-death room mutations at $84:AC05..AC88. Every six-byte
/// program draws one physical layout for one frame, then deletes its PLM.
/// The two cartridge-unused background rows are included because they are
/// contiguous authored instruction data, not adjacent machine code.
/// </summary>
internal static class MotherBrainFakeDeathPlmProgramDefinitions
{
    /// <summary><c>$84:AC05</c>: first fake-death room mutation.</summary>
    internal const ushort Start = RoomPlmInstructionLists.FillMotherBrainsWall;
    /// <summary><c>$84:AC89</c>: first byte after the twenty-two lists.</summary>
    internal const ushort EndExclusive = 0xac89;
    /// <summary>Native size of each timer/draw/delete list in bytes.</summary>
    internal const int ProgramByteLength = 6;

    private static readonly ushort[] DrawPointers =
    [
        MotherBrainFakeDeathPlmDrawDefinitions.FillWall,
        MotherBrainFakeDeathPlmDrawDefinitions.EscapeDoor,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow2,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow3,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow4,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow5,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow6,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow7,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow8,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow9,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowA,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowB,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowC,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowD,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowEUnused,
        MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowFUnused,
        MotherBrainFakeDeathPlmDrawDefinitions.ClearCeilingBlock,
        MotherBrainFakeDeathPlmDrawDefinitions.ClearCeilingTube,
        MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomMiddleSideTube,
        MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomMiddleTubes,
        MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomLeftTube,
        MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomRightTube,
    ];

    internal static int ProgramCount => DrawPointers.Length;

    internal static IEnumerable<ushort> NativeWordAddresses() =>
        Enumerable.Range(0, ProgramCount * 3)
            .Select(index => checked((ushort)(Start + index * 2)));

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - Start;
        if (offset < 0 || address >= EndExclusive || (offset & 1) != 0)
        {
            value = 0;
            return false;
        }
        int programIndex = offset / ProgramByteLength;
        value = (offset % ProgramByteLength) switch
        {
            0 => (ushort)1,
            2 => DrawPointers[programIndex],
            4 => checked((ushort)RoomPlmInstructionCodes.Delete),
            _ => throw new InvalidDataException(
                $"Mother Brain instruction offset {offset} is not a word."),
        };
        return true;
    }
}
