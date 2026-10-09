using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Native object pointers in the authored order used to enumerate the Tourian statue programs.</summary>
    private static readonly ushort[] OriginalTourianObjects = [0x854c, 0x8552, 0x8558, 0x855e];

    /// <summary>Runs the Tourian statue descriptor checks for object identity and each native descriptor field.</summary>
    /// <param name="rom">Cartridge address space supplying the original descriptor operands.</param>
    private static void VerifyTourianStatueDescriptorFields(ISnesAddressSpace rom)
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

    /// <summary>Checks that exactly the four authored object pointers resolve and all other ushort values remain unsupported.</summary>
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

    /// <summary>Checks each descriptor's program start against the native word at its object header.</summary>
    /// <param name="rom">Cartridge address space containing the statue object headers.</param>
    private static void VerifyTourianStatueProgramStarts(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.ProgramStart, true, 0));

    /// <summary>Checks the encoded graphics transfer byte count at each statue descriptor's native offset.</summary>
    /// <param name="rom">Cartridge address space containing the statue descriptors.</param>
    private static void VerifyTourianStatueTransferSizes(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.TransferByteCount, true, 2));

    /// <summary>Checks each descriptor's encoded VRAM destination against its native word.</summary>
    /// <param name="rom">Cartridge address space containing the statue descriptors.</param>
    private static void VerifyTourianStatueVramDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.EncodedVramDestination, true, 4));

    /// <summary>Checks the four native statue state-bit values used to select statue progression state.</summary>
    /// <param name="rom">Cartridge address space containing the statue descriptor fields.</param>
    private static void VerifyTourianStatueStateBits(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.StatueStateBit, false, 0x02, 0x26, 0x5a, 0x60));

    /// <summary>Checks the statue grey-event operands used by the authored program entries.</summary>
    /// <param name="rom">Cartridge address space containing the statue descriptor operands.</param>
    private static void VerifyTourianStatueGreyEvents(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.GreyEventNumber, false, 0x06, 0x56));

    /// <summary>Checks the initial animation-frame duration encoded for each relevant statue entry.</summary>
    /// <param name="rom">Cartridge address space containing the statue descriptor operands.</param>
    private static void VerifyTourianStatueInitialDurations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.FirstFrameDuration, false, 0x0a));

    /// <summary>Checks the packed boss-state test operand in the Tourian statue program.</summary>
    /// <param name="rom">Cartridge address space containing the statue program data.</param>
    private static void VerifyTourianStatueBossTests(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.PackedBossTest, false, 0x20));

    /// <summary>Checks the byte index used by native statue logic to clear palette state.</summary>
    /// <param name="rom">Cartridge address space containing the statue program data.</param>
    private static void VerifyTourianStatuePaletteClears(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.ClearPaletteByteIndex, false, 0x36));

    /// <summary>Checks the unlock-effect parameters encoded for the statue programs that trigger those effects.</summary>
    /// <param name="rom">Cartridge address space containing the statue program data.</param>
    private static void VerifyTourianStatueEffectParameters(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.UnlockEffectParameter, false, 0x42, 0x4a));

    /// <summary>Checks the palette-FX definition pointer selected by the statue program.</summary>
    /// <param name="rom">Cartridge address space containing the statue program data.</param>
    private static void VerifyTourianStatuePaletteFx(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.PaletteFxDefinition, false, 0x4e));

    /// <summary>Checks the target palette byte index written by the native statue logic.</summary>
    /// <param name="rom">Cartridge address space containing the statue program data.</param>
    private static void VerifyTourianStatueTargetPalettes(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyTourianStatueDescriptorField), () => VerifyTourianStatueDescriptorField(rom, x => x.TargetPaletteByteIndex, false, 0x64));

    /// <summary>Compares one selected descriptor field and its mechanics-word aliases with the original ROM at the specified offsets.</summary>
    /// <param name="rom">Cartridge address space containing the object headers and referenced program words.</param>
    /// <param name="field">Descriptor accessor whose value is checked at the supplied offsets.</param>
    /// <param name="header">True when offsets are relative to the object pointer; otherwise they are relative to its program start.</param>
    /// <param name="offsets">Native byte offsets of the field and mechanics words being verified.</param>
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
