namespace SuperMetroid.Core.Gameplay;

/// <summary>The runtime's live WRAM/SRAM requirement, shared by the runtime and the owners it composes.</summary>
internal static class RuntimeMemory
{
    /// <summary>The address space as the mutable memory bus the runtime requires.</summary>
    [CpuBusBoundary("import: resolves the runtime's live WRAM/SRAM bus")]
    internal static ISnesMemoryBus RequireBus(ISnesAddressSpace addressSpace) =>
        SnesMemoryBusRequirement.Require(addressSpace, "The runtime requires live WRAM/SRAM access.");
}
