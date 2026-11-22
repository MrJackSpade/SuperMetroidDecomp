
namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// A SNES CPU address space: the identity an importer, the WRAM image or a test bus is handed around as. Every address space
/// an importer is handed maps the cartridge, so it is a cartridge source by type (#2554), never one only checked at a read.
/// </summary>
/// <remarks>
/// ROM, WRAM, SRAM, and peripheral reads have separate contracts. No generic read or write is
/// exposed here: a caller must declare which source owns the byte it needs.
/// </remarks>
public interface ISnesAddressSpace : IImportCartridgeSource;
