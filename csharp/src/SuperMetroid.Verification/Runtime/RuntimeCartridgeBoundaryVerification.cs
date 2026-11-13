using System.Reflection;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Runtime;

/// <summary>Guards the compiled gameplay assembly's separation from cartridge import and untyped CPU reads.</summary>
internal static class RuntimeCartridgeBoundaryVerification
{
    /// <summary>
    /// Architectural regression guard: the gameplay assembly cannot acquire a ROM
    /// payload or call an import-time cartridge reader, even through the bus type.
    /// Reference diagnostics use the separate AssetExtraction assembly instead.
    /// </summary>
    /// <remarks>
    /// The gameplay assembly is the one the runtime is compiled into (#1495). The CPU address space and VRAM moved to the
    /// import pipeline (#1417, #1422) and the VRAM write queue to test support, so Core declares none of them: their
    /// absence from Core is what is asserted, and the shape checks they once needed as gameplay types (no ROM payload or
    /// untyped read on the bus, no CPU-bus DMA path in VRAM, no memory source for the queue's drain) no longer guard Core.
    /// </remarks>
    internal static void VerifyRuntimeAddressSpaceHasNoCartridgeApi()
    {
        Assembly gameplay = typeof(SuperMetroidRuntime).Assembly;
        AssertTrue(!gameplay.GetTypes().Any(type =>
                type.Name is nameof(CartridgeImportAddressSpace) or nameof(ISnesAddressSpace) or nameof(ISnesMutableMemory)),
            "the CPU address space belongs to the import pipeline, not the gameplay assembly (#1417)");
        AssertTrue(!gameplay.GetTypes().Any(type => type.Name == nameof(SnesVram)),
            "VRAM belongs to the import pipeline, not the gameplay assembly (#1422)");
        AssertTrue(!MetadataShape.AssemblyDeclaresType(gameplay, "SuperMetroid.Core.Hardware.SnesCpuMappedData"),
            "gameplay assembly must not retain a generic dynamic CPU read adapter");
        AssertTrue(!gameplay.GetReferencedAssemblies().Any(
                name => name.Name == "SuperMetroid.AssetExtraction"),
            "gameplay assembly must not reference the cartridge importer");
        AssertTrue(typeof(SuperMetroid.Core.Rooms.Backgrounds.LibraryBackground).Assembly.GetTypes()
                .All(type => !type.Name.StartsWith("LibraryBackgroundProgram", StringComparison.Ordinal)),
            "Core must not contain a library background's command list; only the import pipeline runs it (#1466)");
        AssertEqual(typeof(SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation),
            MetadataShape.ParameterTypes(typeof(SuperMetroid.Core.World.RoomObjectPopulationLoader), "LoadRoomPopulation")[1],
            "the sequential PLM allocator requires typed decoded placements, not a native pointer/read flag");
        foreach (string importOnlyType in new[]
                 { "SuperMetroid.AssetExtraction.Compression.SmCompression", "SuperMetroid.AssetExtraction.ReferenceRendering.RoomRenderer" })
            AssertTrue(gameplay.GetType(importOnlyType) is null,
                $"Core must not compile the import-only {importOnlyType} type");
    }

    /// <summary>Runs the runtime's cartridge and memory boundary checks.</summary>
    [VerificationCommand("--runtime-cartridge-boundary")]
    internal static int RuntimeCartridgeBoundaryCommand()
    {
        SuiteTimingVerification.Suite(nameof(VerifyRuntimeAddressSpaceHasNoCartridgeApi), () => VerifyRuntimeAddressSpaceHasNoCartridgeApi());
        return 0;
    }
}
