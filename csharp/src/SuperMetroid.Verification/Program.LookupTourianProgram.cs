using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static LookupTourianArtworkVerification;
using static LookupTourianDescriptorsVerification;

/// <summary>Checks Tourian statue animated-tile instruction layouts against original bank-$87 programs.</summary>
internal static class LookupTourianProgramVerification
{
    /// <summary>Checks nine source operands per statue and read-only index bounds.</summary>
    private static void VerifyTourianStatueSourceOperandPositions(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)header,
                out var definition), "Original statue resolves");
            ushort[] expected = OriginalTourianFrames(rom, header).Select(x => x.Operand).ToArray();
            AssertEqual(9, expected.Length, "Native timed-frame count");
            AssertEqual(expected.Length, (definition ?? throw new InvalidOperationException("Verification requires definition.")).SourceOperandPointers.Count, "Calculated operand count");
            AssertTrue(expected.SequenceEqual(definition.SourceOperandPointers), "Original enumerated operand positions");
            for (int index = 0; index < expected.Length; index++)
                AssertEqual(expected[index], definition.SourceOperandPointers[index], "Original indexed operand position");
            foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => { _ = definition.SourceOperandPointers[invalid]; },
                    "Original read-only list rejection bounds");
        }
    }

    /// <summary>Compares compiled statue control opcodes with native instruction words.</summary>
    private static void VerifyTourianStatueInstructionOpcodes(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        foreach (var instruction in OriginalTourianInstructions(rom, header).Where(x => x.Code >= 0x8000))
            VerifyTourianStatueOriginalWord(rom, header, instruction.Cursor);
    }

    /// <summary>Compares timed-frame durations after each statue program's initial frame.</summary>
    private static void VerifyTourianStatueSubsequentDurations(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        foreach (var frame in OriginalTourianInstructions(rom, header).Where(x => x.Code < 0x8000).Skip(1))
            VerifyTourianStatueOriginalWord(rom, header, frame.Cursor);
    }

    /// <summary>Checks operands of statue branch and callback instructions.</summary>
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

    /// <summary>Checks the shared busy flag after the statue-specific state-setting operation.</summary>
    private static void VerifyTourianStatueBusyStateOperand(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
        {
            var stateSets = OriginalTourianInstructions(rom, header).Where(x => x.Code == 0x8349).ToArray();
            AssertEqual(2, stateSets.Length, "Native statue bit followed by shared busy flag");
            VerifyTourianStatueOriginalWord(rom, header, (ushort)(stateSets[1].Cursor + 2));
        }
    }

    /// <summary>Checks complete word ownership, including headers and instruction controls but excluding source art.</summary>
    private static void VerifyTourianStatueMechanicsDomain(ISnesAddressSpace rom)
    {
        int total = 0;
        foreach (ushort header in OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)header,
                out var definition), "Native statue resolves");
            var expected = new HashSet<ushort> { header, (ushort)(header + 2), (ushort)(header + 4) };
            foreach (var instruction in OriginalTourianInstructions(rom, header))
                for (int offset = 0; offset < (instruction.Code < 0x8000 ? 2 : instruction.Width); offset += 2)
                    expected.Add((ushort)(instruction.Cursor + offset));
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                bool found = (definition ?? throw new InvalidOperationException("Verification requires definition.")).TryReadMechanicsWord((ushort)value, out ushort word);
                AssertEqual(expected.Contains((ushort)value), found, "Complete native mechanics word domain");
                if (!found) AssertEqual((ushort)0, word, "Odd, source and unrelated words preserve false/zero");
            }
            total += expected.Count;
        }
        AssertEqual(184, total, "All original mechanics words partitioned by named field/control proofs");
    }

    /// <summary>Compares one compiled statue control word with its native bank-$87 value.</summary>
    private static void VerifyTourianStatueOriginalWord(ISnesAddressSpace rom, ushort header, ushort pointer)
    {
        AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)header, out var definition),
            "Original statue resolves for control word");
        AssertTrue((definition ?? throw new InvalidOperationException("Verification requires definition.")).TryReadMechanicsWord(pointer, out ushort actual), "Original control word is compiled");
        AssertEqual(ReadVerificationWord(rom, 0x870000 | pointer), actual, "Original statue control word");
    }

    /// <summary>Runs operand, opcode, duration, branch, state, and domain checks for Tourian statues.</summary>
    internal static void VerifyTourianStatueProgramMappings(ISnesAddressSpace rom)
    {
        VerifyTourianStatueSourceOperandPositions(rom);
        VerifyTourianStatueInstructionOpcodes(rom);
        VerifyTourianStatueSubsequentDurations(rom);
        VerifyTourianStatueBranchDestinations(rom);
        VerifyTourianStatueBusyStateOperand(rom);
        VerifyTourianStatueMechanicsDomain(rom);
    }
}
