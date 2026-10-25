using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// A SNES CPU address space: the identity an importer, the WRAM image or a test bus is handed around as.
/// </summary>
/// <remarks>
/// ROM, WRAM, SRAM, and peripheral reads have separate contracts. No generic read or write is
/// exposed here: a caller must declare which source owns the byte it needs.
/// </remarks>
[CpuBusAccess]
public interface ISnesAddressSpace;
