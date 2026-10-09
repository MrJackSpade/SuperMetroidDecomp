using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Finite adapter for the identified palette-program coverage gap; no ROM or gameplay reads.</summary>
internal static class MotherBrainRoomFlashAudit
{
    /// <summary>Installs the timed palette-entry keys derived from the production importer.</summary>
    /// <param name="exports">Resource index receiving the room-flash identities.</param>
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

    /// <summary>Runs the real extraction path against a source containing only declared synthetic bytes.</summary>
    /// <returns>The serialized room-color declaration document.</returns>
    internal static byte[] ExtractDeclarationDocument() =>
        MotherBrainRoomColorExtractor.Extract(new ConstructedColorSource());

    /// <summary>Checks all fixed palette-program operands against installed room-flash identities.</summary>
    /// <param name="source">Source label attached to any resource finding.</param>
    /// <param name="exports">Installed resource identities.</param>
    /// <param name="report">Audit report receiving reference and coverage results.</param>
    internal static void Inspect(string source, ResourceIndex exports, AuditReport report) =>
        RequireOperands(Enumerable.Range(0, MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            .Select(MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress).ToArray(), source, exports, report);

    /// <summary>Resolves each byte-offset operand to the corresponding installed color pointer.</summary>
    /// <param name="operands">Palette-program byte offsets to check.</param>
    /// <param name="source">Source label attached to findings.</param>
    /// <param name="exports">Installed resource identities.</param>
    /// <param name="report">Audit report receiving references and coverage.</param>
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

    /// <summary>Formats a room-flash pointer using the source bank defined by the cartridge data.</summary>
    /// <param name="pointer">Bank-relative color pointer.</param>
    /// <returns>Canonical resource-index address.</returns>
    private static string Key(ushort pointer) => ResourceIndex.Address(
        MotherBrainRoomColorRomData.SourceBank >> 16, pointer);

    /// <summary>Only declared pointer slots and color payload ranges may be read; mutations are forbidden.</summary>
    private sealed class ConstructedColorSource : ISnesAddressSpace, IImportCartridgeSource
    {
        // A synthetic RGB5 payload, not a native resource identity.
        /// <summary>Safe in-bank pointer used for constructed pointer-table payloads.</summary>
        private const ushort SyntheticPalettePointer = 0xf000;
        /// <summary>Only address bytes belonging to declared pointer operands may use synthetic pointer data.</summary>
        private readonly Dictionary<int, byte> pointerBytes = [];
        /// <summary>Addresses of color payload bytes accepted by the importer.</summary>
        private readonly HashSet<int> colorBytes = [];

        /// <summary>Builds the finite synthetic source ranges consumed by room-color extraction.</summary>
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

        /// <summary>Adds a byte range representing a declared set of palette colors.</summary>
        /// <param name="source">CPU address at the start of the color payload.</param>
        /// <param name="count">Number of color words in the payload.</param>
        private void AddColors(int source, int count)
        {
            for (int index = 0; index < count * sizeof(ushort); index++) colorBytes.Add(source + index);
        }

        /// <summary>Returns a synthetic pointer byte or inert color byte, rejecting undeclared reads.</summary>
        /// <param name="cpuAddress">CPU address requested by the importer.</param>
        /// <returns>The pointer-table byte or zero-valued synthetic color data.</returns>
        public byte ReadCartridgeByte(int cpuAddress) => pointerBytes.TryGetValue(cpuAddress, out byte value)
            ? value : colorBytes.Contains(cpuAddress) ? (byte)0
            : throw new InvalidDataException($"Room-flash audit requested undeclared bytes at {cpuAddress:X6}.");
        /// <summary>Rejects writes because declaration extraction is observational only.</summary>
        /// <param name="address">Requested emulated address.</param>
        /// <param name="value">Requested byte value.</param>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Static color extraction cannot mutate emulated state.");
    }
}
