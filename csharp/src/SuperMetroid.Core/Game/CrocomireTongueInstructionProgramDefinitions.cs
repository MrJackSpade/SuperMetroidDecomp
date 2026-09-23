namespace SuperMetroid.Core.Game;

/// <summary>One compiled Crocomire-tongue mechanics word at its bank-$A4 address.</summary>
internal readonly record struct CrocomireTongueInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Crocomire's independently scheduled tongue. Interleaved
/// extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CrocomireTongueInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_CrocomireTongue_Fight</c> at $A4:BE56. In the pinned NTSC
    /// J/U v1.0 ROM, the four mechanics words at $BE56 + 4*i (i=0..3) are
    /// exactly five-frame durations; $BE66 is goto $80ED and $BE68 targets
    /// $BE56. The interleaved pointer operands are live presentation data,
    /// and the unused reverse list beginning at $BE6A is outside this loop.
    /// </summary>
    internal const ushort Fight = 0xbe56;
    /// <summary><c>InstList_Crocomire_Sleep</c> at $A4:BF62.</summary>
    internal const ushort Sleep = 0xbf62;
    /// <summary><c>InstList_CrocomireTongue_Melting</c> at $A4:BF98.</summary>
    internal const ushort Melting = 0xbf98;

    private static readonly CrocomireTongueInstructionMechanicsWord[] Words =
    [
        new(0xbe56, 0x0005), new(0xbe5a, 0x0005),
        new(0xbe5e, 0x0005), new(0xbe62, 0x0005),
        new(0xbe66, CommonEnemyInstructionCodes.Goto), new(0xbe68, Fight),
        new(0xbf62, CommonEnemyInstructionCodes.Sleep),
        new(0xbf98, 0x0005), new(0xbf9c, 0x0005), new(0xbfa0, 0x0005),
        new(0xbfa4, 0x0005), new(0xbfa8, 0x0005),
        new(0xbfac, CommonEnemyInstructionCodes.Goto), new(0xbfae, Melting),
    ];

    /// <summary>
    /// The first four entries identify the live fight-loop presentation operands
    /// at $A4:BE58 + 4*i for i=0..3. In the pinned NTSC J/U v1.0 ROM each
    /// unsigned pointer is exactly $C65E + 10*i; $A4:BE66 is the following
    /// goto opcode. Production reads these operands from the live cartridge so
    /// installed spritemap presentation remains effective.
    /// The last five entries identify the melting-loop operands at
    /// $A4:BF9A + 4*i for i=0..4. Their stock pointers are exactly
    /// $CACE + 10*i, followed by the goto opcode at $A4:BFAC. This second
    /// independently indexed loop also keeps its presentation reads live.
    /// </summary>
    private static readonly ushort[] PresentationWords =
    [
        0xbe58, 0xbe5c, 0xbe60, 0xbe64,
        0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CrocomireTongueInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed tongue control or rejects pointers outside its live programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CrocomireTongueInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Crocomire tongue instruction mechanics pointer $A4:{address:X4} is not compiled.");
    }
}
