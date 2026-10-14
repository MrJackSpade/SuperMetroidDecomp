namespace SuperMetroid.Core.Hardware;

/// <summary>The mutable WRAM/SRAM bus a typed domain memory view reads and writes (#629).</summary>
[CpuBusAccess]
public interface ISnesMemoryBus : ISnesAddressSpace, ISnesMutableMemory;
