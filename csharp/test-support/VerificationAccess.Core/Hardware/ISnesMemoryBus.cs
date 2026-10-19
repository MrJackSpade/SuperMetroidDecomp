namespace SuperMetroid.Core.Hardware;

/// <summary>The mutable WRAM/SRAM bus a typed domain memory view reads and writes (#629); only verification writes.</summary>
[CpuBusAccess]
internal interface ISnesMemoryBus : ISnesWritableAddressSpace, ISnesMutableMemory;
