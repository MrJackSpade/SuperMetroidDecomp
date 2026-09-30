using System.Reflection;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Architectural regression guard: the gameplay assembly cannot acquire a ROM
    /// payload or call an import-time cartridge reader, even through the bus type.
    /// Reference diagnostics use the separate AssetExtraction assembly instead.
    /// </summary>
    private static void VerifyRuntimeAddressSpaceHasNoCartridgeApi()
    {
        Type runtimeBus = typeof(SuperMetroidAddressSpace);
        const BindingFlags publicInstance = BindingFlags.Public | BindingFlags.Instance;
        AssertTrue(runtimeBus.GetProperty("Rom", publicInstance) is null,
            "runtime address space must not expose cartridge bytes");
        AssertTrue(runtimeBus.GetMethod("ReadByte", publicInstance) is null,
            "runtime address space must not expose an untyped CPU read");
        AssertTrue(runtimeBus.GetMethod("ReadCartridgeByte", publicInstance) is null,
            "runtime address space must not expose a cartridge read");
        AssertTrue(typeof(ISnesAddressSpace).GetMethod("ReadByte") is null,
            "gameplay bus contract must not expose an untyped read");
        AssertTrue(typeof(ISnesAddressSpace).GetMethod("ReadCartridgeByte") is null,
            "gameplay bus contract must not expose a cartridge read");
        AssertTrue(runtimeBus.Assembly.GetType("SuperMetroid.Core.Hardware.SnesCpuMappedData") is null,
            "gameplay assembly must not retain a generic dynamic CPU read adapter");
        AssertTrue(!runtimeBus.Assembly.GetReferencedAssemblies().Any(
                name => name.Name == "SuperMetroid.AssetExtraction"),
            "gameplay assembly must not reference the cartridge importer");
        AssertTrue(typeof(SnesVram).GetMethod("ExecuteQueuedWrite") is null &&
                typeof(SnesVram).GetMethod("ExecuteHardwareDmaWrite") is null,
            "VRAM must not retain untyped CPU-bus DMA APIs");
        AssertEqual(typeof(ISnesMutableMemory), typeof(SnesVram)
            .GetMethod(nameof(SnesVram.ExecuteQueuedMemoryWrite))!.GetParameters()[0].ParameterType,
            "queued memory DMA requires a compile-time mutable-memory source");
        AssertEqual(typeof(ISnesMutableMemory), typeof(SnesVram)
            .GetMethod(nameof(SnesVram.ExecuteHardwareMemoryDmaWrite))!.GetParameters()[0].ParameterType,
            "hardware memory DMA requires a compile-time mutable-memory source");
        AssertEqual(typeof(ISnesMutableMemory), typeof(VramWriteQueue)
            .GetMethod(nameof(VramWriteQueue.DrainTo))!.GetParameters()[1].ParameterType,
            "NMI queue drainage requires a compile-time mutable-memory source");
        AssertTrue(typeof(SuperMetroid.Core.Rooms.LibraryBackgroundLoader)
                .GetMethod("ExecuteNativeForVerification", BindingFlags.Static | BindingFlags.NonPublic) is null,
            "Core must not contain a native background byte decoder, even behind a diagnostic-only entry point");
        AssertEqual(typeof(SuperMetroid.Core.Rooms.RoomPlmPopulationDefinition),
            typeof(SuperMetroid.Core.Rooms.RoomPlmSystem)
                .GetMethod("LoadRoomPopulation")!.GetParameters()[4].ParameterType,
            "the sequential PLM allocator requires typed decoded placements, not a native pointer/read flag");
        foreach (string importOnlyType in new[]
                 { "SuperMetroid.Core.Assets.SmCompression", "SuperMetroid.Core.Rooms.RoomRenderer" })
            AssertTrue(runtimeBus.Assembly.GetType(importOnlyType) is null,
                $"Core must not compile the import-only {importOnlyType} type");
    }
}
