using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AnimatedTileInstructionCodesVerification;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents lookup tourian descriptors verification.</summary>
internal static class LookupTourianDescriptorsVerification
{
    /// <summary>Stores original tourian objects.</summary>
    internal static readonly ushort[] OriginalTourianObjects = [0x854c, 0x8552, 0x8558, 0x855e];

    /// <summary>Verifies tourian statue descriptor fields.</summary>
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

    /// <summary>Verifies tourian statue object domain.</summary>
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
            if (found) AssertEqual(pointer, (ushort)definition.ObjectPointer, "Selected statue identity");
            else AssertTrue(definition is null, "Unknown statue preserves false/null contract");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)undefined, out _),
                "Undefined statue object is rejected at the typed boundary");
    }

    /// <summary>Verifies tourian statue program starts.</summary>
    private static void VerifyTourianStatueProgramStarts(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.ProgramStart, true, 0));
    /// <summary>Verifies tourian statue transfer sizes.</summary>
    private static void VerifyTourianStatueTransferSizes(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.TransferByteCount, true, 2));
    /// <summary>Verifies tourian statue vram destinations.</summary>
    private static void VerifyTourianStatueVramDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.EncodedVramDestination, true, 4));
    /// <summary>Verifies tourian statue state bits.</summary>
    private static void VerifyTourianStatueStateBits(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.StatueStateBit, false, 0x02, 0x26, 0x5a, 0x60));
    /// <summary>Verifies tourian statue grey events.</summary>
    private static void VerifyTourianStatueGreyEvents(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.GreyEventNumber, false, 0x06, 0x56));
    /// <summary>Verifies tourian statue initial durations.</summary>
    private static void VerifyTourianStatueInitialDurations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.FirstFrameDuration, false, 0x0a));
    /// <summary>Verifies tourian statue boss tests.</summary>
    private static void VerifyTourianStatueBossTests(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.PackedBossTest, false, 0x20));
    /// <summary>Verifies tourian statue palette clears.</summary>
    private static void VerifyTourianStatuePaletteClears(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.ClearPaletteByteIndex, false, 0x36));
    /// <summary>Verifies tourian statue effect parameters.</summary>
    private static void VerifyTourianStatueEffectParameters(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.UnlockEffectParameter, false, 0x42, 0x4a));
    /// <summary>Verifies tourian statue palette fx.</summary>
    private static void VerifyTourianStatuePaletteFx(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.PaletteFxDefinition, false, 0x4e));
    /// <summary>Verifies tourian statue target palettes.</summary>
    private static void VerifyTourianStatueTargetPalettes(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.TargetPaletteByteIndex, false, 0x64));

    /// <summary>Verifies tourian statue descriptor field.</summary>
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
            AssertEqual(expected, field(definition), "Original descriptor field");
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
