using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Decode the original instructions, not the replacement operand roster or offsets.
    // Widths follow bank_87.asm at revision 362be646929cf8e483f692b73a6561cfc2dc1d0d.
    /// <summary>
    /// Enumerates each native timed-frame operand and the source address stored at that operand
    /// for the selected Tourian statue program.
    /// </summary>
    /// <param name="rom">Address space containing the original bank-$87 program and artwork pointers.</param>
    /// <param name="header">Bank-local object-header pointer selecting the statue program.</param>
    private static IEnumerable<(ushort Operand, int Source)> OriginalTourianFrames(
        ISnesAddressSpace rom, ushort header)
    {
        foreach (var instruction in OriginalTourianInstructions(rom, header))
            if (instruction.Code < 0x8000)
                yield return ((ushort)(instruction.Cursor + 2),
                    0x870000 | ReadVerificationWord(rom, 0x870000 | (instruction.Cursor + 2)));
    }

    /// <summary>
    /// Walks the original statue instruction stream from its header until both delete opcodes
    /// are encountered, yielding each instruction's cursor, opcode, and encoded width.
    /// </summary>
    /// <param name="rom">Address space containing the original bank-$87 instruction stream.</param>
    /// <param name="header">Bank-local object-header pointer to the instruction stream.</param>
    private static IEnumerable<(ushort Cursor, ushort Code, int Width)> OriginalTourianInstructions(
        ISnesAddressSpace rom, ushort header)
    {
        int cursor = ReadVerificationWord(rom, 0x870000 | header);
        int deletes = 0;
        for (int count = 0; deletes < 2; count++)
        {
            AssertTrue(count < 64, "Original statue program has bounded instruction count");
            ushort code = ReadVerificationWord(rom, 0x870000 | cursor);
            if (code < 0x8000)
            {
                yield return ((ushort)cursor, code, 4);
                cursor += 4;
                continue;
            }
            if (code == 0x80b2) deletes++;
            int width = code switch
            {
                0x80b2 => 2,
                0x813f or 0x8303 => 6,
                0x80b7 or 0x8150 or 0x8320 or 0x832f or 0x833e or
                    0x8349 or 0x8352 or 0x835b or 0x8372 or 0x837f => 4,
                _ => throw new InvalidDataException($"Unexpected native statue instruction {code:X4}."),
            };
            yield return ((ushort)cursor, code, width);
            cursor += width;
        }
    }

    /// <summary>
    /// Checks all nine timed-frame source mappings for each of the four stock Tourian statues
    /// against the original ROM, and verifies that unrelated operands and unknown definitions
    /// are rejected.
    /// </summary>
    /// <param name="rom">Address space containing the original statue programs and source pointers.</param>
    private static void VerifyTourianStatueArtworkSources(ISnesAddressSpace rom)
    {
        int total = 0;
        foreach (ushort header in new ushort[] { 0x854c, 0x8552, 0x8558, 0x855e })
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(header,
                out var definition), "Original statue object resolves");
            var expected = OriginalTourianFrames(rom, header).ToDictionary(x => x.Operand, x => x.Source);
            AssertEqual(9, expected.Count, "Nine original timed statue frames");
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                ushort operand = (ushort)value;
                if (expected.TryGetValue(operand, out int source))
                    AssertEqual(source, TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition, operand),
                        "Original statue artwork operand including repeated/released frames");
                else
                    AssertThrows<InvalidDataException>(
                        () => TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition, operand),
                        "All non-frame, odd and neighboring program addresses reject");
            }
            total += expected.Count;
        }
        AssertEqual(36, total, "Complete native statue artwork domain");
        AssertThrows<ArgumentNullException>(
            () => TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(null!, 0), "Null statue rejects");
        var unknown = new TourianStatueAnimatedTileProgramDefinition(0, 0x8000, 0x40, 0,
            0, 0, 0, 0, 0, 0, 0, 0);
        AssertThrows<InvalidDataException>(
            () => TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(unknown, 0x800c),
            "Unknown object rejects even with its own valid operand");
    }
}
