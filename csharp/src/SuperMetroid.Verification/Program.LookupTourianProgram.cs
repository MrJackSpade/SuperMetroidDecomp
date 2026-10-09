using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies that the compiled statue program exposes native timed-frame source operand positions.</summary>
    /// <param name="rom">Address space used to enumerate the original instruction stream for comparison.</param>
    private static void VerifyTourianStatueSourceOperandPositions(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(header,
                out var definition), "Original statue resolves");
            ushort[] expected = OriginalTourianFrames(rom, header).Select(x => x.Operand).ToArray();
            AssertEqual(9, expected.Length, "Native timed-frame count");
            AssertEqual(expected.Length, definition.SourceOperandPointers.Count, "Calculated operand count");
            AssertTrue(expected.SequenceEqual(definition.SourceOperandPointers), "Original enumerated operand positions");
            for (int index = 0; index < expected.Length; index++)
                AssertEqual(expected[index], definition.SourceOperandPointers[index], "Original indexed operand position");
            foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => { _ = definition.SourceOperandPointers[invalid]; },
                    "Original read-only list rejection bounds");
        }
    }

    /// <summary>Compares each original statue instruction opcode with its compiled mechanics word.</summary>
    /// <param name="rom">Address space containing the original statue instruction streams.</param>
    private static void VerifyTourianStatueInstructionOpcodes(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        foreach (var instruction in OriginalTourianInstructions(rom, header).Where(x => x.Code >= 0x8000))
            VerifyTourianStatueOriginalWord(rom, header, instruction.Cursor);
    }

    /// <summary>Checks the duration words for every timed frame after the first compiled frame.</summary>
    /// <param name="rom">Address space containing the original statue instruction streams.</param>
    private static void VerifyTourianStatueSubsequentDurations(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        foreach (var frame in OriginalTourianInstructions(rom, header).Where(x => x.Code < 0x8000).Skip(1))
            VerifyTourianStatueOriginalWord(rom, header, frame.Cursor);
    }

    /// <summary>Verifies the compiled branch and loop destination operands used by statue programs.</summary>
    /// <param name="rom">Address space containing the original statue instruction streams.</param>
    private static void VerifyTourianStatueBranchDestinations(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        foreach (var instruction in OriginalTourianInstructions(rom, header))
        {
            int operand = instruction.Code switch
            {
                0x813f or 0x8303 => 4,
                0x80b7 or 0x833e => 2,
                _ => 0,
            };
            if (operand != 0)
                VerifyTourianStatueOriginalWord(rom, header, (ushort)(instruction.Cursor + operand));
        }
    }

    /// <summary>Checks the operand that points from each statue's state instruction to the shared busy flag.</summary>
    /// <param name="rom">Address space containing the original statue instruction streams.</param>
    private static void VerifyTourianStatueBusyStateOperand(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        {
            var stateSets = OriginalTourianInstructions(rom, header).Where(x => x.Code == 0x8349).ToArray();
            AssertEqual(2, stateSets.Length, "Native statue bit followed by shared busy flag");
            VerifyTourianStatueOriginalWord(rom, header, (ushort)(stateSets[1].Cursor + 2));
        }
    }

    /// <summary>Exhaustively confirms each compiled statue mechanics definition accepts exactly its native control-word domain.</summary>
    /// <param name="rom">Address space used to enumerate each statue's original instructions.</param>
    private static void VerifyTourianStatueMechanicsDomain(ISnesAddressSpace rom)
    {
        int total = 0;
        foreach (ushort header in OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(header,
                out var definition), "Native statue resolves");
            var expected = new HashSet<ushort> { header, (ushort)(header + 2), (ushort)(header + 4) };
            foreach (var instruction in OriginalTourianInstructions(rom, header))
                for (int offset = 0; offset < (instruction.Code < 0x8000 ? 2 : instruction.Width); offset += 2)
                    expected.Add((ushort)(instruction.Cursor + offset));
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                bool found = definition.TryReadMechanicsWord((ushort)value, out ushort word);
                AssertEqual(expected.Contains((ushort)value), found, "Complete native mechanics word domain");
                if (!found) AssertEqual((ushort)0, word, "Odd, source and unrelated words preserve false/zero");
            }
            total += expected.Count;
        }
        AssertEqual(184, total, "All original mechanics words partitioned by named field/control proofs");
    }

    /// <summary>Compares one compiled statue control word with the corresponding word in the original bank.</summary>
    /// <param name="rom">Address space containing the original control word.</param>
    /// <param name="header">Native object-header pointer selecting the statue definition.</param>
    /// <param name="pointer">Bank-local address of the mechanics word to compare.</param>
    private static void VerifyTourianStatueOriginalWord(ISnesAddressSpace rom, ushort header, ushort pointer)
    {
        AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(header, out var definition),
            "Original statue resolves for control word");
        AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort actual), "Original control word is compiled");
        AssertEqual(ReadVerificationWord(rom, 0x870000 | pointer), actual, "Original statue control word");
    }

    /// <summary>Runs the focused source-position, opcode, timing, branch, busy-state, and mechanics-domain checks.</summary>
    /// <param name="rom">Address space used to compare compiled statue data with the original programs.</param>
    private static void VerifyTourianStatueProgramMappings(ISnesAddressSpace rom)
    {
        VerifyTourianStatueSourceOperandPositions(rom);
        VerifyTourianStatueInstructionOpcodes(rom);
        VerifyTourianStatueSubsequentDurations(rom);
        VerifyTourianStatueBranchDestinations(rom);
        VerifyTourianStatueBusyStateOperand(rom);
        VerifyTourianStatueMechanicsDomain(rom);
    }
}
