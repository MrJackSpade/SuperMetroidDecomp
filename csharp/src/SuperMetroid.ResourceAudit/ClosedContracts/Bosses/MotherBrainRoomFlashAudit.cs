using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Finite adapter for the identified palette-program coverage gap; no ROM or gameplay reads.</summary>
internal static class MotherBrainRoomFlashAudit
{
    /// <summary>Loads a constructed room-color document and exports its actual timed flash-entry keys for audit membership.</summary>
    internal static void Install(ResourceIndex exports)
    {
        // The importer maps each declared pointer operand to a JSON row; the
        // loader validates every row and exposes its actual timed-entry key.
        // Synthetic pointers/colors exercise only this declaration contract.
        byte[] document = ExtractDeclarationDocument();
        MotherBrainRoomColorPresentation installed = MotherBrainRoomColorPresentation.Load(new MemoryStream(document));
        foreach (ushort pointer in installed.FlashEntryPointers)
            exports.Add(ResourceDomains.MotherBrainRoomFlash, Key(pointer));
    }

    /// <summary>Exercises the palette importer with declared pointer slots and bounded synthetic RGB5 payloads.</summary>
    internal static byte[] ExtractDeclarationDocument() =>
        MotherBrainRoomColorExtractor.Extract(ConstructedColorSource());

    /// <summary>Checks all compiled Mother Brain flash presentation operands against the exported entry domain.</summary>
    internal static void Inspect(string source, ResourceIndex exports, AuditReport report) =>
        RequireOperands(Enumerable.Range(0, MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            .Select(MotherBrainRoomColorExtractor.FlashPaletteOperandAddress).ToArray(), source, exports, report);

    /// <summary>Converts palette operand addresses to timed-entry keys and records resolved coverage for the resource audit.</summary>
    internal static void RequireOperands(ReadOnlySpan<ushort> operands, string source,
        ResourceIndex exports, AuditReport report)
    {
        int before = report.ReferenceCount;
        foreach (ushort operand in operands)
            report.Require(ResourceDomains.MotherBrainRoomFlash, nameof(MotherBrainRoomPaletteProgramDefinitions),
                Key(checked((ushort)(operand - MotherBrainRoomColorRomDataTooling.PaletteOperandByteOffset))), source, exports);
        report.Coverage.Add(new(ResourceDomains.MotherBrainRoomFlash, report.ReferenceCount - before,
            exports.Count(ResourceDomains.MotherBrainRoomFlash)));
    }

    /// <summary>Formats a Mother Brain room-flash pointer with its native source bank for resource indexing.</summary>
    private static string Key(ushort pointer) => ResourceIndex.Address(
        MotherBrainRoomColorRomData.SourceBank >> 16, pointer);

    /// <summary>Only declared pointer slots and color payload ranges may be read; mutations are forbidden.</summary>
    [CpuBusBoundary("exact bus test: declares the Mother Brain room palette pointer slots and color ranges the importer may read")]
    private static SyntheticCartridge ConstructedColorSource()
    {
        // A synthetic RGB5 payload, not a native resource identity.
        const ushort syntheticPalettePointer = 0xf000;
        var source = new SyntheticCartridge(
            address => new InvalidDataException($"Room-flash audit requested undeclared bytes at {address:X6}."),
            _ => new InvalidOperationException("Static color extraction cannot mutate emulated state."));
        for (int index = 0; index < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount; index++)
            source.SetWord(MotherBrainRoomColorRomData.SourceBank |
                MotherBrainRoomColorExtractor.FlashPaletteOperandAddress(index), syntheticPalettePointer);
        void AddColors(int start, int count) => source.DeclareZero(start, count * sizeof(ushort));
        AddColors(MotherBrainRoomColorRomData.SourceBank | syntheticPalettePointer,
            MotherBrainRoomColorRomData.SliceColors * 2);
        AddColors(MotherBrainRoomColorRomData.SourceBank | MotherBrainRoomFlashSourceDefinitions.FinalPalette,
            MotherBrainRoomColorRomData.SliceColors * 2);
        AddColors(MotherBrainRoomColorRomData.PhaseTwoAttackSource, MotherBrainRoomColorRomData.PhaseTwoColors);
        AddColors(MotherBrainRoomColorRomData.PhaseTwoRearLegSource, MotherBrainRoomColorRomData.PhaseTwoColors);
        AddColors(MotherBrainRoomColorRomData.InitialGlassShardSource, MotherBrainRoomColorRomData.InitialColors);
        AddColors(MotherBrainRoomColorRomData.InitialTubeProjectileSource, MotherBrainRoomColorRomData.InitialColors);
        for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsFrames; index++)
            AddColors(MotherBrainRoomColorRomData.RecoveryLightsSource(index),
                MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination * 2);
        return source;
    }
}
