using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Import-time cartridge image and mutable memory. Gameplay code cannot reference this
/// project, so owning the image here prevents a runtime address space from reading ROM.
/// </summary>
public sealed class CartridgeImportAddressSpace : SuperMetroidAddressSpace,
    IImportCartridgeSource, IRestoredSharedContent
{
    /// <summary>Size in bytes of the unheadered retail Super Metroid ROM image: 3 MiB.</summary>
    public const int RetailRomByteCount = 0x300000;

    // The immutable image is shared through CartridgeImageCache; debugger graphs carry its digest.
    [NonSerialized] internal byte[] _rom;
    private readonly byte[] _romSha256;

    /// <summary>Creates mutable import memory backed by a shared immutable LoROM image.</summary>
    /// <param name="unheaderedRom">Cartridge bytes without a copier header, containing 1-128 complete 32 KiB banks.</param>
    /// <exception cref="ArgumentException">The image is empty, exceeds 4 MiB, or is not a multiple of 32 KiB.</exception>
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

    /// <summary>Reads the immutable image through an upper-window LoROM CPU address and its mirrored banks.</summary>
    /// <param name="cpuAddress">A 24-bit CPU address with a bank offset of <c>$8000-$FFFF</c>, outside WRAM banks <c>$7E-$7F</c>.</param>
    /// <returns>The cartridge byte mapped to the supplied address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The address is not 24-bit, selects a lower bank window, or selects a WRAM bank.</exception>
    /// <exception cref="InvalidOperationException">The mapped byte falls beyond the loaded image.</exception>
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

    /// <summary>Maps an upper-window LoROM address to a zero-based image offset, folding bank mirrors into the 4 MiB image window.</summary>
    /// <param name="address">A 24-bit address whose bank offset is <c>$8000-$FFFF</c>.</param>
    /// <returns>The offset formed from 32 KiB banks, with the bank's mirror bit removed.</returns>
    /// <remarks>This arithmetic mapping does not check WRAM bank identity or whether an image contains the resulting offset.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The address is not 24-bit or selects the lower bank window.</exception>
    public static int ToRomOffset(int address)
    {
        if ((uint)address > 0x00ff_ffff || (address & 0x8000) == 0)
            throw new ArgumentOutOfRangeException(nameof(address), address,
                "Address is outside the upper LoROM window.");
        int bank = address >> 16;
        return ((bank << 15) | (address & 0x7fff)) & 0x3f_ffff;
    }
}
