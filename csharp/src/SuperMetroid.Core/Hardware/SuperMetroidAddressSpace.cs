namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Mutable work RAM and save RAM portions of Super Metroid's 24-bit CPU address space.
/// Cartridge ROM belongs to the asset importer, never to the gameplay address space.
/// </summary>
/// <remarks>
/// Hardware registers and cartridge windows are intentionally unmapped.
/// </remarks>
public sealed class SuperMetroidAddressSpace : ISnesAddressSpace, ISnesMutableMemory
{
    /// <summary>Two complete 64 KiB WRAM banks, <c>$7E</c> and <c>$7F</c>.</summary>
    public const int WorkRamByteCount = 0x20000;

    /// <summary>8 KiB of battery-backed SRAM used by three save slots and metadata.</summary>
    public const int SaveRamByteCount = 0x2000;

    private readonly byte[] _workRam = new byte[WorkRamByteCount];
    private readonly byte[] _saveRam = new byte[SaveRamByteCount];

    /// <summary>
    /// Creates the mutable WRAM/SRAM portion of the runtime address space without a
    /// cartridge payload. This is used by ROM-independent save validation and will still
    /// fail loudly if code accidentally attempts to read a cartridge address.
    /// </summary>
    public static SuperMetroidAddressSpace CreateWithoutCartridge() =>
        new();

    /// <summary>Mutable physical WRAM used by translated routines and save-state tools.</summary>
    public Span<byte> WorkRam => _workRam;

    /// <summary>Mutable battery-backed SRAM; persistence remains an explicit host concern.</summary>
    public Span<byte> SaveRam => _saveRam;

    /// <inheritdoc />
    public byte ReadWorkRamByte(int cpuAddress)
    {
        ValidateAddress(cpuAddress);
        int bank = cpuAddress >> 16;
        int offset = cpuAddress & 0xffff;
        if (bank is 0x7e or 0x7f)
            return _workRam[(bank - 0x7e) * 0x10000 + offset];
        if (IsSystemBank(bank) && offset < LoRomExpansionReadMap.WorkRamMirrorEnd)
            return _workRam[offset];
        throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
            "WRAM reads require bank $7E/$7F or a low-window system-bank mirror.");
    }

    /// <inheritdoc />
    public byte ReadSaveRamByte(int cpuAddress)
    {
        ValidateAddress(cpuAddress);
        int bank = cpuAddress >> 16;
        int offset = cpuAddress & 0xffff;
        if (!IsSaveRamBank(bank) || offset >= LoRomExpansionReadMap.RomStart)
            throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
                "SRAM reads require a lower-window bank $70-$7D or $F0-$FF address.");
        return _saveRam[offset & 0x1fff];
    }

    /// <summary>
    /// Writes the currently supported mutable regions: WRAM and SRAM.
    /// </summary>
    public void WriteByte(int address, byte value)
    {
        ValidateAddress(address);
        int bank = address >> 16;
        int offset = address & 0xffff;

        if (bank is 0x7e or 0x7f)
        {
            _workRam[(bank - 0x7e) * 0x10000 + offset] = value;
            return;
        }

        if (IsSystemBank(bank) && offset < LoRomExpansionReadMap.WorkRamMirrorEnd)
        {
            _workRam[offset] = value;
            return;
        }

        if (IsSaveRamBank(bank) && offset < 0x8000)
        {
            _saveRam[offset & 0x1fff] = value;
            return;
        }

        if (offset >= 0x8000)
            throw new InvalidOperationException($"Cannot write cartridge ROM at ${bank:X2}:{offset:X4}.");

        throw new InvalidOperationException(
            $"CPU write ${bank:X2}:{offset:X4} is outside the runtime address map.");
    }

    private static bool IsSystemBank(int bank) => bank <= 0x3f || bank is >= 0x80 and <= 0xbf;

    private static bool IsSaveRamBank(int bank) => bank is >= 0x70 and <= 0x7d or >= 0xf0 and <= 0xff;

    private static void ValidateAddress(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address), address, "SNES CPU address must fit in 24 bits.");
    }
}
