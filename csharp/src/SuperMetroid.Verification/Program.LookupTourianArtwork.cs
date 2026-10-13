using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;

/// <summary>Checks Tourian statue animation artwork operands against decoded native instructions.</summary>
internal static class LookupTourianArtworkVerification
{
    /// <summary>Enumerates native timed statue frames and their bank-$87 artwork source addresses.</summary>
    internal static IEnumerable<(ushort Operand, int Source)> OriginalTourianFrames(
        ISnesAddressSpace rom, ushort header)
    {
        foreach (var instruction in OriginalTourianInstructions(rom, header))
            if (instruction.Code < 0x8000)
                yield return ((ushort)(instruction.Cursor + 2),
                    0x870000 | ReadVerificationWord(rom, 0x870000 | (instruction.Cursor + 2)));
    }

    /// <summary>Decodes native statue instruction widths through both delete commands, rejecting unknown opcodes.</summary>
    internal static IEnumerable<(ushort Cursor, ushort Code, int Width)> OriginalTourianInstructions(
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

    /// <summary>Checks all 36 frame operands and rejects every non-frame address or unknown statue object.</summary>
    internal static void VerifyTourianStatueArtworkSources(ISnesAddressSpace rom)
    {
        int total = 0;
        foreach (ushort header in new ushort[] { 0x854c, 0x8552, 0x8558, 0x855e })
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)header,
                out var definition), "Original statue object resolves");
            var expected = OriginalTourianFrames(rom, header).ToDictionary(x => x.Operand, x => x.Source);
            AssertEqual(9, expected.Count, "Nine original timed statue frames");
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                ushort operand = (ushort)value;
                if (expected.TryGetValue(operand, out int source))
                    AssertEqual(source, TourianStatueAnimatedTileArtworkDefinitions.SourceAddress((definition ?? throw new InvalidOperationException("Verification requires definition.")), operand),
                        "Original statue artwork operand including repeated/released frames");
                else
                    AssertThrows<InvalidDataException>(
                        () => TourianStatueAnimatedTileArtworkDefinitions.SourceAddress((definition ?? throw new InvalidOperationException("Verification requires definition.")), operand),
                        "All non-frame, odd and neighboring program addresses reject");
            }
            total += expected.Count;
        }
        AssertEqual(36, total, "Complete native statue artwork domain");
        var unknown = new TourianStatueAnimatedTileProgramDefinition(new TourianStatueAnimatedTileProgramParts { ObjectPointer = 0, ProgramStart = 0x8000, TransferByteCount = 0x40, EncodedVramDestination = 0, StatueStateBit = 0, GreyEventNumber = 0, FirstFrameDuration = 0, PackedBossTest = 0, ClearPaletteByteIndex = 0, UnlockEffectParameter = 0, PaletteFxDefinition = 0, TargetPaletteByteIndex = 0 });
        AssertThrows<InvalidDataException>(
            () => TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(unknown, 0x800c),
            "Unknown object rejects even with its own valid operand");
    }
}
