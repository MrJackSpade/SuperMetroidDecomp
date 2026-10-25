namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>The mutually exclusive mapped sources a PPU DMA transfer can read.</summary>
internal enum SnesDmaSourceKind
{
    /// <summary>Direct WRAM bank or its low-bank mirror supplies the DMA byte.</summary>
    WorkRam,
    /// <summary>The upper LoROM cartridge window supplies the DMA byte.</summary>
    Cartridge,
    /// <summary>No mapped memory source owns this bank and offset.</summary>
    Unmapped,
}
