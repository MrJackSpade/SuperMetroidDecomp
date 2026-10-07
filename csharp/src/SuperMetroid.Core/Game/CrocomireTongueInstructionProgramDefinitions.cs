namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Crocomire's independently scheduled tongue. Interleaved
/// extended-spritemap operands are compiled selectors for installed artwork;
/// constructed no-art fixtures may still supply mutable cartridge data.
/// </summary>
internal abstract class CrocomireTongueInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>
    /// <c>InstList_CrocomireTongue_Fight</c> at $A4:BE56. In the pinned NTSC
    /// J/U v1.0 ROM, the four mechanics words at $BE56 + 4*i (i=0..3) are
    /// exactly five-frame durations; $BE66 is goto $80ED and $BE68 targets
    /// $BE56. The interleaved pointer operands are presentation selectors,
    /// and the unused reverse list beginning at $BE6A is outside this loop.
    /// </summary>
    internal const ushort Fight = 0xbe56;
    /// <summary>
    /// <c>InstList_Crocomire_Sleep</c> at $A4:BF62 contains the single
    /// $812F Sleep opcode in the pinned NTSC J/U v1.0 ROM. Bridge collapse
    /// selects it for the invisible tongue; Sleep holds this cursor, so
    /// $BF64 and the following body-melting list are never read as tongue
    /// instructions. The terminal case returns the named Sleep opcode.
    /// </summary>
    internal const ushort Sleep = 0xbf62;
    /// <summary>
    /// <c>InstList_CrocomireTongue_Melting</c> at $A4:BF98. Its five
    /// mechanics words at $BF98 + 4*i (i=0..4) are exactly five-frame
    /// durations in the pinned NTSC J/U v1.0 ROM. $BFAC is goto $80ED and
    /// $BFAE targets $BF98; $BFB0 begins a different body program. The
    /// interleaved spritemap pointers remain presentation selectors.
    /// </summary>
    internal const ushort Melting = 0xbf98;

    public static int MechanicsWordCount => 14;
    public static int PresentationWordCount => 9;

    /// <summary>Enumerates four fight durations and their loop, terminal sleep,
    /// then five melting durations and their loop, in native address order.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index == 6) return new(Sleep, CommonEnemyInstructionCodes.Sleep);
        int start = index < 6 ? Fight : Melting;
        int frameCount = index < 6 ? 4 : 5;
        int field = index < 6 ? index : index - 7;
        int offset = field < frameCount ? 4 * field : 4 * frameCount + 2 * (field - frameCount);
        ushort address = (ushort)(start + offset);
        return new(address, ReadMechanicsWord(address));
    }

    /// <summary>Spritemap operand positions in the four-frame fight and five-frame
    /// melting loops: start + 2 + 4*frame. These positions do not own artwork.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? Fight + 2 + 4 * index : Melting + 2 + 4 * (index - 4));
    }

    /// <summary>Dispatches five-frame durations, self-loop control, and terminal
    /// sleep. Only exact mechanics addresses are accepted; interleaved sprite
    /// selectors, odd addresses, and adjacent lists remain outside this contract.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Sleep) return CommonEnemyInstructionCodes.Sleep;
        int start = address < Melting ? Fight : Melting;
        int frameCount = address < Melting ? 4 : 5;
        int offset = address - start;
        if (offset >= 0 && offset < frameCount * 4 && offset % 4 == 0) return 5;
        if (offset == frameCount * 4) return CommonEnemyInstructionCodes.Goto;
        if (offset == frameCount * 4 + 2) return (ushort)start;
        throw new InvalidDataException(
            $"Crocomire tongue instruction mechanics pointer $A4:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa40000) return false;
        int bankAddress = address & 0xffff;
        if (bankAddress == Sleep || bankAddress == Sleep + 1) return true;
        int offset = bankAddress < Melting ? bankAddress - Fight : bankAddress - Melting;
        int frameCount = bankAddress < Melting ? 4 : 5;
        return offset >= 0 && offset < frameCount * 4 + 4 &&
            (offset >= frameCount * 4 || offset % 4 < 2);
    }
}
