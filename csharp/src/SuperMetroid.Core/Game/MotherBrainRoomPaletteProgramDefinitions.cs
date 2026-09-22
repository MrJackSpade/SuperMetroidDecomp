namespace SuperMetroid.Core.Game;

internal readonly record struct MotherBrainRoomPaletteMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control words for Mother Brain's fake-death room-palette flash program.
/// Palette pointers and their BGR555 payloads remain live cartridge presentation data.
/// </summary>
internal static class MotherBrainRoomPaletteProgramDefinitions
{
    /// <summary><c>MotherBrain_FakeDeath_RoomPalette_InstructionList</c> at $A9:D046.</summary>
    public const ushort FlashStart = 0xd046;

    /// <summary><c>Instruction_Goto</c> at $A9:9B0F.</summary>
    public const ushort GotoInstruction = 0x9b0f;

    /// <summary>The final grey room palette at $A9:D082.</summary>
    public const ushort FinalPalette = 0xd082;

    /// <summary>Fake-death grey-fade palette pointer table at $AD:ED8A.</summary>
    public const ushort GrayFadePointerTable = 0xed8a;

    private static readonly MotherBrainRoomPaletteMechanicsWord[] Words =
    [
        new(0xd046, 0x0002), new(0xd04a, 0x0002), new(0xd04e, 0x0002),
        new(0xd052, 0x0002), new(0xd056, 0x0002), new(0xd05a, 0x0002),
        new(0xd05e, 0x0002), new(0xd062, 0x0002), new(0xd066, 0x0002),
        new(0xd06a, 0x0002), new(0xd06e, 0x0002), new(0xd072, 0x0002),
        new(0xd076, 0x0002), new(0xd07a, 0x0002), new(0xd07e, 0x9b0f),
        new(0xd080, 0xd046),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd048, 0xd04c, 0xd050, 0xd054, 0xd058, 0xd05c, 0xd060,
        0xd064, 0xd068, 0xd06c, 0xd070, 0xd074, 0xd078, 0xd07c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MotherBrainRoomPaletteMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MotherBrainRoomPaletteMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Mother Brain room-palette mechanics pointer $A9:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool TryGetPresentationWord(int address, out ushort wordAddress)
    {
        wordAddress = 0;
        if ((address & 0xff0000) != 0xa90000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PresentationWords.Length; index++)
        {
            ushort candidate = PresentationWords[index];
            if (bankAddress == candidate ||
                bankAddress == unchecked((ushort)(candidate + 1)))
            {
                wordAddress = candidate;
                return true;
            }
        }
        return false;
    }
}
