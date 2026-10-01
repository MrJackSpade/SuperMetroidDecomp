using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Finite adapter for the identified palette-program coverage gap; no ROM or gameplay reads.</summary>
internal static class MotherBrainRoomFlashAudit
{
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

    internal static byte[] ExtractDeclarationDocument() =>
        MotherBrainRoomColorExtractor.Extract(new ConstructedColorSource());

    internal static void Inspect(string source, ResourceIndex exports, AuditReport report) =>
        RequireOperands(Enumerable.Range(0, MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            .Select(MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress).ToArray(), source, exports, report);

    internal static void RequireOperands(ReadOnlySpan<ushort> operands, string source,
        ResourceIndex exports, AuditReport report)
    {
        int before = report.ReferenceCount;
        foreach (ushort operand in operands)
            report.Require(ResourceDomains.MotherBrainRoomFlash, nameof(MotherBrainRoomPaletteProgramDefinitions),
                Key(checked((ushort)(operand - MotherBrainRoomColorRomData.PaletteOperandByteOffset))), source, exports);
        report.Coverage.Add(new(ResourceDomains.MotherBrainRoomFlash, report.ReferenceCount - before,
            exports.Count(ResourceDomains.MotherBrainRoomFlash)));
    }

    private static string Key(ushort pointer) => ResourceIndex.Address(
        MotherBrainRoomColorRomData.SourceBank >> 16, pointer);

    /// <summary>Only declared pointer slots and color payload ranges may be read; mutations are forbidden.</summary>
    private sealed class ConstructedColorSource : ISnesAddressSpace, IImportCartridgeSource
    {
        // A synthetic RGB5 payload, not a native resource identity.
        private const ushort SyntheticPalettePointer = 0xf000;
        private readonly Dictionary<int, byte> pointerBytes = [];
        private readonly HashSet<int> colorBytes = [];

        public ConstructedColorSource()
        {
            for (int index = 0; index < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount; index++)
            {
                int address = MotherBrainRoomColorRomData.SourceBank |
                    MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(index);
                pointerBytes.Add(address, unchecked((byte)SyntheticPalettePointer));
                pointerBytes.Add(address + 1, (byte)(SyntheticPalettePointer >> 8));
            }
            AddColors(MotherBrainRoomColorRomData.SourceBank | SyntheticPalettePointer,
                MotherBrainRoomColorRomData.SliceColors * 2);
            AddColors(MotherBrainRoomColorRomData.SourceBank | MotherBrainRoomPaletteProgramDefinitions.FinalPalette,
                MotherBrainRoomColorRomData.SliceColors * 2);
            AddColors(MotherBrainRoomColorRomData.PhaseTwoAttackSource, MotherBrainRoomColorRomData.PhaseTwoColors);
            AddColors(MotherBrainRoomColorRomData.PhaseTwoRearLegSource, MotherBrainRoomColorRomData.PhaseTwoColors);
            AddColors(MotherBrainRoomColorRomData.InitialGlassShardSource, MotherBrainRoomColorRomData.InitialColors);
            AddColors(MotherBrainRoomColorRomData.InitialTubeProjectileSource, MotherBrainRoomColorRomData.InitialColors);
            for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsFrames; index++)
                AddColors(MotherBrainRoomColorRomData.RecoveryLightsSource(index),
                    MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination * 2);
        }

        private void AddColors(int source, int count)
        {
            for (int index = 0; index < count * sizeof(ushort); index++) colorBytes.Add(source + index);
        }

        public byte ReadCartridgeByte(int cpuAddress) => pointerBytes.TryGetValue(cpuAddress, out byte value)
            ? value : colorBytes.Contains(cpuAddress) ? (byte)0
            : throw new InvalidDataException($"Room-flash audit requested undeclared bytes at {cpuAddress:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Static color extraction cannot mutate emulated state.");
    }
}
