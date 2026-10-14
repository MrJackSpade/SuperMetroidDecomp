using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AnimatedTileInstructionCodesVerification;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Compares the four Tourian statue animated-tile descriptors and their mechanics aliases with bank $87.</summary>
internal static class LookupTourianDescriptorsVerification
{
    /// <summary>The four native object headers used to enumerate statue descriptors in cartridge order.</summary>
    internal static readonly ushort[] OriginalTourianObjects = [0x854c, 0x8552, 0x8558, 0x855e];

    /// <summary>Runs field-specific checks for each statue descriptor's program, transfer, state, and effects.</summary>
    internal static void VerifyTourianStatueDescriptorFields(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyTourianStatueObjectDomain), () => VerifyTourianStatueObjectDomain());
        Suite(nameof(VerifyTourianStatueProgramStarts), () => VerifyTourianStatueProgramStarts(rom));
        Suite(nameof(VerifyTourianStatueTransferSizes), () => VerifyTourianStatueTransferSizes(rom));
        Suite(nameof(VerifyTourianStatueVramDestinations), () => VerifyTourianStatueVramDestinations(rom));
        Suite(nameof(VerifyTourianStatueStateBits), () => VerifyTourianStatueStateBits(rom));
        Suite(nameof(VerifyTourianStatueGreyEvents), () => VerifyTourianStatueGreyEvents(rom));
        Suite(nameof(VerifyTourianStatueInitialDurations), () => VerifyTourianStatueInitialDurations(rom));
        Suite(nameof(VerifyTourianStatueBossTests), () => VerifyTourianStatueBossTests(rom));
        Suite(nameof(VerifyTourianStatuePaletteClears), () => VerifyTourianStatuePaletteClears(rom));
        Suite(nameof(VerifyTourianStatueEffectParameters), () => VerifyTourianStatueEffectParameters(rom));
        Suite(nameof(VerifyTourianStatuePaletteFx), () => VerifyTourianStatuePaletteFx(rom));
        Suite(nameof(VerifyTourianStatueTargetPalettes), () => VerifyTourianStatueTargetPalettes(rom));
    }

    /// <summary>Checks exact object-header coverage and the false/null and invalid-value lookup contracts.</summary>
    private static void VerifyTourianStatueObjectDomain()
    {
        AssertTrue(OriginalTourianObjects.SequenceEqual(
            TourianStatueAnimatedTileMechanicsDefinitions.All.Select(x => (ushort)x.ObjectPointer)),
            "Original statue descriptor enumeration order");
        var original = OriginalTourianObjects.ToHashSet();
        foreach (AnimatedTileObject header in Enum.GetValues<AnimatedTileObject>())
        {
            ushort pointer = (ushort)header;
            bool found = TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(header, out var definition);
            AssertEqual(original.Contains(pointer), found, "Complete statue object identity domain");
            if (found) AssertEqual(pointer, (ushort)(definition ?? throw new InvalidOperationException("Verification requires definition.")).ObjectPointer, "Selected statue identity");
            else AssertTrue(definition is null, "Unknown statue preserves false/null contract");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)undefined, out _),
                "Undefined statue object is rejected at the typed boundary");
    }

    /// <summary>Compares each animated-tile program pointer in the native descriptor header.</summary>
    private static void VerifyTourianStatueProgramStarts(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.ProgramStart, true, 0));
    /// <summary>Compares byte counts for statue graphics transfers.</summary>
    private static void VerifyTourianStatueTransferSizes(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.TransferByteCount, true, 2));
    /// <summary>Compares the encoded VRAM destination in each descriptor header.</summary>
    private static void VerifyTourianStatueVramDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.EncodedVramDestination, true, 4));
    /// <summary>Checks statue-state bits and their aliased busy-bit reset masks.</summary>
    private static void VerifyTourianStatueStateBits(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.StatueStateBit, false, 0x02, 0x26, 0x5a, 0x60));
    /// <summary>Checks event numbers used when statue figures turn grey.</summary>
    private static void VerifyTourianStatueGreyEvents(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.GreyEventNumber, false, 0x06, 0x56));
    /// <summary>Checks each statue's initial animated-tile frame duration.</summary>
    private static void VerifyTourianStatueInitialDurations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.FirstFrameDuration, false, 0x0a));
    /// <summary>Checks the packed boss-defeat tests used by statue mechanics.</summary>
    private static void VerifyTourianStatueBossTests(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.PackedBossTest, false, 0x20));
    /// <summary>Checks the palette byte indices cleared as statues unlock.</summary>
    private static void VerifyTourianStatuePaletteClears(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.ClearPaletteByteIndex, false, 0x36));
    /// <summary>Checks the effect parameters used by the statue unlock sequence.</summary>
    private static void VerifyTourianStatueEffectParameters(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.UnlockEffectParameter, false, 0x42, 0x4a));
    /// <summary>Checks each statue's palette FX definition pointer.</summary>
    private static void VerifyTourianStatuePaletteFx(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.PaletteFxDefinition, false, 0x4e));
    /// <summary>Checks the target palette byte index for each statue's unlock effect.</summary>
    private static void VerifyTourianStatueTargetPalettes(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.TargetPaletteByteIndex, false, 0x64));

    /// <summary>Compares one typed descriptor field and all listed mechanics aliases with native words.</summary>
    private static void VerifyTourianStatueDescriptorField(ISnesAddressSpace rom,
        Func<TourianStatueAnimatedTileProgramDefinition, ushort> field, bool header, params int[] offsets)
    {
        var enumerated = TourianStatueAnimatedTileMechanicsDefinitions.All.ToDictionary(x => x.ObjectPointer);
        foreach (ushort pointer in OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)pointer, out var definition),
                "Native descriptor resolves");
            int origin = header ? pointer : ReadVerificationWord(rom, 0x870000 | pointer);
            ushort expected = ReadVerificationWord(rom, 0x870000 | (origin + offsets[0]));
            AssertEqual(expected, field((definition ?? throw new InvalidOperationException("Verification requires definition."))), "Original descriptor field");
            AssertEqual(expected, field(enumerated[(AnimatedTileObject)pointer]), "Original enumerated descriptor field");
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
