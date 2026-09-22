namespace SuperMetroid.Core.Game;

/// <summary>One compiled gunship mechanics word at its native bank-$A2 address.</summary>
internal readonly record struct GunshipInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Named enemy definitions for the three-part Landing Site gunship actor.</summary>
internal static class GunshipEnemyDefinitions
{
    /// <summary>Enemy definition <c>EnemyDefs_ShipTop</c> at $A0:D07F.</summary>
    public const ushort Top = 0xd07f;
    /// <summary>Enemy definition <c>EnemyDefs_ShipBottomEntrance</c> at $A0:D0BF.</summary>
    public const ushort BottomEntrance = 0xd0bf;
}

/// <summary>
/// Compiled engine-control words for the gunship hull and entrance-pad programs.
/// Interleaved spritemap pointers remain live cartridge presentation data.
/// </summary>
internal static class GunshipInstructionProgramDefinitions
{
    /// <summary><c>InstList_ShipEntrancePad_Opening_0</c> at $A2:A5BE.</summary>
    public const ushort EntrancePadOpening = 0xa5be;
    /// <summary><c>InstList_ShipEntrancePad_Opening_1</c> at $A2:A5E6.</summary>
    public const ushort EntrancePadOpen = 0xa5e6;
    /// <summary><c>InstList_ShipEntrancePad_Closing</c> at $A2:A5EE.</summary>
    public const ushort EntrancePadClosing = 0xa5ee;
    /// <summary><c>InstList_ShipEntrancePad_Closed</c> at $A2:A60E.</summary>
    public const ushort BottomEntrancePad = 0xa60e;
    /// <summary><c>InstList_ShipTop</c> at $A2:A616.</summary>
    public const ushort TopHull = 0xa616;
    /// <summary><c>InstList_ShipBottom</c> at $A2:A61C.</summary>
    public const ushort BottomHull = 0xa61c;

    private static readonly GunshipInstructionMechanicsWord[] Words =
    [
        new(0xa5be, 0x0028), new(0xa5c2, 0x0008), new(0xa5c6, 0x0008),
        new(0xa5ca, 0x0008), new(0xa5ce, 0x0018), new(0xa5d2, 0x0008),
        new(0xa5d6, 0x0007), new(0xa5da, 0x0006), new(0xa5de, 0x0005),
        new(0xa5e2, 0x0004), new(0xa5e6, 0x0004),
        new(0xa5ea, CommonEnemyInstructionCodes.Goto), new(0xa5ec, EntrancePadOpen),
        new(0xa5ee, 0x0004), new(0xa5f2, 0x0005), new(0xa5f6, 0x0006),
        new(0xa5fa, 0x0007), new(0xa5fe, 0x0008), new(0xa602, 0x0018),
        new(0xa606, 0x0008), new(0xa60a, 0x0008), new(0xa60e, 0x0008),
        new(0xa612, CommonEnemyInstructionCodes.Goto),
        new(0xa614, BottomEntrancePad),
        new(0xa616, 0x0001), new(0xa61a, CommonEnemyInstructionCodes.Sleep),
        new(0xa61c, 0x0001), new(0xa620, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xa5c0, 0xa5c4, 0xa5c8, 0xa5cc, 0xa5d0, 0xa5d4, 0xa5d8,
        0xa5dc, 0xa5e0, 0xa5e4, 0xa5e8,
        0xa5f0, 0xa5f4, 0xa5f8, 0xa5fc, 0xa600, 0xa604, 0xa608, 0xa60c,
        0xa610, 0xa618, 0xa61e,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static GunshipInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GunshipInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Gunship instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
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
}
