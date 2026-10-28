using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
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
    internal static void VerifyRuntimeAddressSpaceHasNoCartridgeApi()
    {
        Assembly gameplay = typeof(SnesVram).Assembly;
        AssertTrue(!gameplay.GetTypes().Any(type =>
                type.Name is nameof(SuperMetroidAddressSpace) or nameof(ISnesAddressSpace) or nameof(ISnesMutableMemory)),
            "the CPU address space belongs to the import pipeline, not the gameplay assembly (#1417)");
        Type runtimeBus = typeof(SuperMetroidAddressSpace);
        const BindingFlags publicInstance = BindingFlags.Public | BindingFlags.Instance;
        AssertTrue(!MetadataShape.DeclaresProperty(runtimeBus, "Rom", publicInstance),
            "runtime address space must not expose cartridge bytes");
        AssertTrue(!MetadataShape.DeclaresMethod(runtimeBus, "ReadByte", publicInstance),
            "runtime address space must not expose an untyped CPU read");
        AssertTrue(!MetadataShape.DeclaresMethod(runtimeBus, "ReadCartridgeByte", publicInstance),
            "runtime address space must not expose a cartridge read");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(ISnesAddressSpace), "ReadByte"),
            "gameplay bus contract must not expose an untyped read");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(ISnesAddressSpace), "ReadCartridgeByte"),
            "gameplay bus contract must not expose a cartridge read");
        AssertTrue(!MetadataShape.AssemblyDeclaresType(gameplay, "SuperMetroid.Core.Hardware.SnesCpuMappedData"),
            "gameplay assembly must not retain a generic dynamic CPU read adapter");
        AssertTrue(!gameplay.GetReferencedAssemblies().Any(
                name => name.Name == "SuperMetroid.AssetExtraction"),
            "gameplay assembly must not reference the cartridge importer");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(SnesVram), "ExecuteQueuedWrite") &&
                !MetadataShape.DeclaresMethod(typeof(SnesVram), "ExecuteHardwareDmaWrite"),
            "VRAM must not retain untyped CPU-bus DMA APIs");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(SnesVram), "ExecuteQueuedMemoryWrite") &&
                !MetadataShape.DeclaresMethod(typeof(SnesVram), "ExecuteDmaWrite"),
            "VRAM must not retain a memory-sourced DMA path; queued transfers come from installed artwork");
        AssertTrue(!MetadataShape.ParameterTypes(typeof(VramWriteQueue), nameof(VramWriteQueue.DrainTo))
                .Any(type => typeof(ISnesMemoryBus).IsAssignableFrom(type) || typeof(ISnesMutableMemory).IsAssignableFrom(type)),
            "NMI queue drainage takes no memory source");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(SuperMetroid.Core.World.LibraryBackgroundLoader), "ExecuteNativeForVerification", BindingFlags.Static | BindingFlags.NonPublic),
            "Core must not contain a native background byte decoder, even behind a diagnostic-only entry point");
        AssertEqual(typeof(SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinition),
            MetadataShape.ParameterTypes(typeof(SuperMetroid.Core.World.PlmPopulationLoader), "LoadRoomPopulation")[1],
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
