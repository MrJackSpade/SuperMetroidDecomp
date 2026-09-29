using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Import-time cartridge image and mutable memory. Gameplay code cannot reference this
/// project, so owning the image here prevents a runtime address space from reading ROM.
/// </summary>
public sealed class CartridgeImportAddressSpace : SuperMetroidAddressSpace,
    IImportCartridgeSource
{
    public const int RetailRomByteCount = 0x300000;

    private readonly byte[] _rom;

    public CartridgeImportAddressSpace(ReadOnlySpan<byte> unheaderedRom)
    {
        if (unheaderedRom.IsEmpty || unheaderedRom.Length > 0x400000 ||
            (unheaderedRom.Length & 0x7fff) != 0)
            throw new ArgumentException(
                "ROM must contain one to 128 complete 32 KiB LoROM banks.",
                nameof(unheaderedRom));

        _rom = unheaderedRom.ToArray();
    }

    public ReadOnlySpan<byte> Rom => _rom;

    public static CartridgeImportAddressSpace LoadRetailRom(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] fileBytes = File.ReadAllBytes(path);
        ReadOnlySpan<byte> romBytes = fileBytes.Length switch
        {
            RetailRomByteCount => fileBytes,
            RetailRomByteCount + 512 => fileBytes.AsSpan(512),
            _ => throw new InvalidDataException(
                $"Expected a ${RetailRomByteCount:X} byte retail ROM (optionally plus a 512-byte copier header), " +
                $"but '{path}' contains ${fileBytes.Length:X} bytes.")
        };

        if (!romBytes.Slice(0x7fc0, "Super Metroid"u8.Length).SequenceEqual("Super Metroid"u8))
            throw new InvalidDataException($"'{path}' does not contain the expected SUPER METROID LoROM header title.");

        return new CartridgeImportAddressSpace(romBytes);
    }

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
