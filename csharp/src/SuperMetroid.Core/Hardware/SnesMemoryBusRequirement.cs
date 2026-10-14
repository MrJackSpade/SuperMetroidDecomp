namespace SuperMetroid.Core.Hardware;

/// <summary>Resolves the live WRAM/SRAM bus that typed domain memory views require.</summary>
public static class SnesMemoryBusRequirement
{
    /// <summary>
    /// Returns <paramref name="bus"/> as the mutable memory bus, or throws naming the
    /// <paramref name="requirement"/> when the address space cannot supply live memory.
    /// </summary>
    public static ISnesMemoryBus Require(ISnesAddressSpace bus, string requirement)
    {
        Ensure.NotNull(bus);
        return bus as ISnesMemoryBus ?? throw new InvalidOperationException(requirement);
    }
}
