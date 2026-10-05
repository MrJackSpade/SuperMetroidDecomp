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
/// Interleaved visual selectors are compiled identities; their editable OAM
/// compositions are installed assets, not live cartridge reads.
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

    internal static int MechanicsWordCount => 28;
    internal static int PresentationWordCount => 22;

    /// <summary>Opening begins with a40-tick wait and a24-tick intermediate hold, then accelerates8..4; closing reverses the transition.</summary>
    private static ushort OpeningDuration(int frame) => (ushort)(frame switch
    {
        0 => 40,
        4 => 24,
        _ => Math.Clamp(13 - frame, 4, 8),
    });

    internal static GunshipInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 11) return new((ushort)(EntrancePadOpening + 4 * index), OpeningDuration(index));
        if (index < 13) return index == 11
            ? new(EntrancePadOpen + 4, CommonEnemyInstructionCodes.Goto)
            : new(EntrancePadOpen + 6, EntrancePadOpen);
        if (index < 22) return new((ushort)(EntrancePadClosing + 4 * (index - 13)), OpeningDuration(9 - (index - 13)));
        if (index < 24) return index == 22
            ? new(BottomEntrancePad + 4, CommonEnemyInstructionCodes.Goto)
            : new(BottomEntrancePad + 6, BottomEntrancePad);
        int local = index - 24;
        return new((ushort)(TopHull + 6 * (local / 2) + 4 * (local % 2)),
            local % 2 == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 11) return (ushort)(EntrancePadOpening + 2 + 4 * index);
        if (index < 20) return (ushort)(EntrancePadClosing + 2 + 4 * (index - 11));
        return (ushort)(TopHull + 2 + 6 * (index - 20));
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GunshipInstructionMechanicsWord candidate = MechanicsWord(middle);
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
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
