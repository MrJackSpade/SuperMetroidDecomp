namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="WramWord"/> that only verification reads.</summary>
[CpuBusAccess]
internal static class WramWordConstants
{
    /// <summary>The word at an element address already bounds-checked by its table.</summary>
    internal static WramWord AtElement(ISnesMemoryBus bus, int address) =>
        new(bus, CpuBusCellConstants.WramWord($"table element ${address:X6}", address));
}
