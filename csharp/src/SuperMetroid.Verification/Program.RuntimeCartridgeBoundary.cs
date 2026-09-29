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
        AssertTrue(!runtimeBus.Assembly.GetReferencedAssemblies().Any(
                name => name.Name == "SuperMetroid.AssetExtraction"),
            "gameplay assembly must not reference the cartridge importer");
    }
}
