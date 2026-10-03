using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly ushort[] OriginalTourianObjects = [0x854c, 0x8552, 0x8558, 0x855e];

    private static void VerifyTourianStatueDescriptorFields(ISnesAddressSpace rom)
    {
        VerifyTourianStatueObjectDomain();
        VerifyTourianStatueProgramStarts(rom);
        VerifyTourianStatueTransferSizes(rom);
        VerifyTourianStatueVramDestinations(rom);
        VerifyTourianStatueStateBits(rom);
        VerifyTourianStatueGreyEvents(rom);
        VerifyTourianStatueInitialDurations(rom);
        VerifyTourianStatueBossTests(rom);
        VerifyTourianStatuePaletteClears(rom);
        VerifyTourianStatueEffectParameters(rom);
        VerifyTourianStatuePaletteFx(rom);
        VerifyTourianStatueTargetPalettes(rom);
    }

    private static void VerifyTourianStatueObjectDomain()
    {
        AssertTrue(OriginalTourianObjects.SequenceEqual(
            TourianStatueAnimatedTileMechanicsDefinitions.All.Select(x => x.ObjectPointer)),
            "Original statue descriptor enumeration order");
        var original = OriginalTourianObjects.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool found = TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(pointer, out var definition);
            AssertEqual(original.Contains(pointer), found, "Complete statue object identity domain");
            if (found) AssertEqual(pointer, definition.ObjectPointer, "Selected statue identity");
            else AssertTrue(definition is null, "Unknown statue preserves false/null contract");
        }
    }

    private static void VerifyTourianStatueProgramStarts(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.ProgramStart, true, 0);
    private static void VerifyTourianStatueTransferSizes(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.TransferByteCount, true, 2);
    private static void VerifyTourianStatueVramDestinations(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.EncodedVramDestination, true, 4);
    private static void VerifyTourianStatueStateBits(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.StatueStateBit, false, 0x02, 0x26, 0x5a, 0x60);
    private static void VerifyTourianStatueGreyEvents(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.GreyEventNumber, false, 0x06, 0x56);
    private static void VerifyTourianStatueInitialDurations(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.FirstFrameDuration, false, 0x0a);
    private static void VerifyTourianStatueBossTests(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.PackedBossTest, false, 0x20);
    private static void VerifyTourianStatuePaletteClears(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.ClearPaletteByteIndex, false, 0x36);
    private static void VerifyTourianStatueEffectParameters(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.UnlockEffectParameter, false, 0x42, 0x4a);
    private static void VerifyTourianStatuePaletteFx(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.PaletteFxDefinition, false, 0x4e);
    private static void VerifyTourianStatueTargetPalettes(ISnesAddressSpace rom) =>
        VerifyTourianStatueDescriptorField(rom, x => x.TargetPaletteByteIndex, false, 0x64);

    private static void VerifyTourianStatueDescriptorField(ISnesAddressSpace rom,
        Func<TourianStatueAnimatedTileProgramDefinition, ushort> field, bool header, params int[] offsets)
    {
        var enumerated = TourianStatueAnimatedTileMechanicsDefinitions.All.ToDictionary(x => x.ObjectPointer);
        foreach (ushort pointer in OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(pointer, out var definition),
                "Native descriptor resolves");
            int origin = header ? pointer : ReadVerificationWord(rom, 0x870000 | pointer);
            ushort expected = ReadVerificationWord(rom, 0x870000 | (origin + offsets[0]));
            AssertEqual(expected, field(definition), "Original descriptor field");
            AssertEqual(expected, field(enumerated[pointer]), "Original enumerated descriptor field");
            foreach (int offset in offsets)
            {
                ushort address = (ushort)(origin + offset);
                AssertTrue(definition.TryReadMechanicsWord(address, out ushort word), "Field mechanics view exists");
                AssertEqual(ReadVerificationWord(rom, 0x870000 | address), word,
                    "Original field mechanics alias, including busy-bit reset masks");
            }
        }
    }
}
