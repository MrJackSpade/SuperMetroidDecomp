using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Import-time cartridge image and mutable memory. Gameplay code cannot reference this
/// project, so owning the image here prevents a runtime address space from reading ROM.
/// </summary>
public sealed class CartridgeImportAddressSpace : SuperMetroidAddressSpace,
    IImportCartridgeSource, IRestoredSharedContent
{
    public const int RetailRomByteCount = 0x300000;

    // The immutable image is shared through CartridgeImageCache; debugger graphs carry its digest.
    [NonSerialized] internal byte[] _rom;
    private readonly byte[] _romSha256;

    public CartridgeImportAddressSpace(ReadOnlySpan<byte> unheaderedRom)
    {
        if (unheaderedRom.IsEmpty || unheaderedRom.Length > 0x400000 ||
            (unheaderedRom.Length & 0x7fff) != 0)
            throw new ArgumentException(
                "ROM must contain one to 128 complete 32 KiB LoROM banks.",
                nameof(unheaderedRom));

        _rom = CartridgeImageCache.Share(unheaderedRom, out _romSha256);
    }

    void IRestoredSharedContent.ReattachSharedContent() => _rom = CartridgeImageCache.Resolve(_romSha256);

    public byte ReadCartridgeByte(int cpuAddress)
    {
        int bank = cpuAddress >> 16;
        if ((uint)cpuAddress > 0x00ff_ffff || bank is 0x7e or 0x7f ||
            (cpuAddress & 0x8000) == 0)
            throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
                "Cartridge reads require an upper-window LoROM address outside WRAM banks.");

        int offset = ToRomOffset(cpuAddress);
        if ((uint)offset >= _rom.Length)
            throw new InvalidOperationException(
                $"Cartridge address ${bank:X2}:{cpuAddress & 0xffff:X4} is not populated.");
        return _rom[offset];
    }

    public static int ToRomOffset(int address)
    {
        if ((uint)address > 0x00ff_ffff || (address & 0x8000) == 0)
            throw new ArgumentOutOfRangeException(nameof(address), address,
                "Address is outside the upper LoROM window.");
        int bank = address >> 16;
        return ((bank << 15) | (address & 0x7fff)) & 0x3f_ffff;
    }
}
