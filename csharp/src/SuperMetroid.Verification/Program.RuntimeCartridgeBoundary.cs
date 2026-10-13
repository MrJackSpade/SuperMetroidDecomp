using System.Reflection;
using SuperMetroid.Core.Hardware;

internal static class RuntimeCartridgeBoundaryVerification
{
    /// <summary>
    /// Architectural regression guard: the gameplay assembly cannot acquire a ROM
    /// payload or call an import-time cartridge reader, even through the bus type.
    /// Reference diagnostics use the separate AssetExtraction assembly instead.
    /// </summary>
    internal static void VerifyRuntimeAddressSpaceHasNoCartridgeApi()
    {
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
        AssertTrue(!MetadataShape.AssemblyDeclaresType(runtimeBus.Assembly, "SuperMetroid.Core.Hardware.SnesCpuMappedData"),
            "gameplay assembly must not retain a generic dynamic CPU read adapter");
        AssertTrue(!runtimeBus.Assembly.GetReferencedAssemblies().Any(
                name => name.Name == "SuperMetroid.AssetExtraction"),
            "gameplay assembly must not reference the cartridge importer");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(SnesVram), "ExecuteQueuedWrite") &&
                !MetadataShape.DeclaresMethod(typeof(SnesVram), "ExecuteHardwareDmaWrite"),
            "VRAM must not retain untyped CPU-bus DMA APIs");
        AssertEqual(typeof(ISnesMutableMemory), MetadataShape.ParameterTypes(typeof(SnesVram), nameof(SnesVram.ExecuteQueuedMemoryWrite))[0],
            "queued memory DMA requires a compile-time mutable-memory source");
        AssertEqual(typeof(ISnesMutableMemory), MetadataShape.ParameterTypes(typeof(VramWriteQueue), nameof(VramWriteQueue.DrainTo))[1],
            "NMI queue drainage requires a compile-time mutable-memory source");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(SuperMetroid.Core.Rooms.LibraryBackgroundLoader), "ExecuteNativeForVerification", BindingFlags.Static | BindingFlags.NonPublic),
            "Core must not contain a native background byte decoder, even behind a diagnostic-only entry point");
        AssertEqual(typeof(SuperMetroid.Core.Rooms.RoomPlmPopulationDefinition),
            MetadataShape.ParameterTypes(typeof(SuperMetroid.Core.Rooms.PlmPopulationLoader), "LoadRoomPopulation")[1],
            "the sequential PLM allocator requires typed decoded placements, not a native pointer/read flag");
        foreach (string importOnlyType in new[]
                 { "SuperMetroid.Core.Assets.SmCompression", "SuperMetroid.Core.Rooms.RoomRenderer" })
            AssertTrue(runtimeBus.Assembly.GetType(importOnlyType) is null,
                $"Core must not compile the import-only {importOnlyType} type");
    }
}
