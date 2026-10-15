namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Mutable work RAM and save RAM portions of Super Metroid's 24-bit CPU address space.
/// Cartridge ROM belongs to the asset importer, never to the gameplay address space.
/// </summary>
/// <remarks>
/// Hardware registers and cartridge windows are intentionally unmapped.
/// </remarks>
[CpuBusBoundary("CPU-bus implementation: maps WRAM banks, low-window mirrors and SRAM windows.")]
public class SuperMetroidAddressSpace : ISnesMemoryBus
{
    /// <summary>Two complete 64 KiB WRAM banks, <c>$7E</c> and <c>$7F</c>.</summary>
    public const int WorkRamByteCount = 2 * SnesBusLayout.BankBytes;

    /// <summary>8 KiB of battery-backed SRAM used by three save slots and metadata.</summary>
    public const int SaveRamByteCount = SnesBusLayout.SaveRamBytes;

    /// <summary>Backing bytes for both full WRAM banks and their low-window system-bank mirrors.</summary>
    internal readonly byte[] _workRam = new byte[WorkRamByteCount];
    /// <summary>Backing bytes shared by the mirrored lower-window save-bank addresses.</summary>
    internal readonly byte[] _saveRam = new byte[SaveRamByteCount];

    /// <summary>
    /// Creates the mutable WRAM/SRAM portion of the runtime address space without a
    /// cartridge payload. This is used by ROM-independent save validation and will still
    /// fail loudly if code accidentally attempts to read a cartridge address.
    /// </summary>
    public static SuperMetroidAddressSpace CreateWithoutCartridge() =>
        new();

    /// <inheritdoc />
    public byte ReadWorkRamByte(int cpuAddress)
    {
        ValidateAddress(cpuAddress);
        int bank = SnesAddressMath.BankNumber(cpuAddress);
        int offset = SnesAddressMath.OffsetInBank(cpuAddress);
        if (SnesBusLayout.IsWorkRamBank(bank))
            return _workRam[(bank - SnesBusLayout.FirstWorkRamBank) * SnesBusLayout.BankBytes + offset];
        if (SnesBusLayout.IsSystemBank(bank) && offset < SnesBusLayout.WorkRamMirrorEnd)
            return _workRam[offset];
        throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
            "WRAM reads require bank $7E/$7F or a low-window system-bank mirror.");
    }

    /// <inheritdoc />
    public byte ReadSaveRamByte(int cpuAddress)
    {
        ValidateAddress(cpuAddress);
        int bank = SnesAddressMath.BankNumber(cpuAddress);
        int offset = SnesAddressMath.OffsetInBank(cpuAddress);
        if (!SnesBusLayout.IsSaveRamBank(bank) || offset >= SnesBusLayout.RomWindowStart)
            throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
                "SRAM reads require a lower-window bank $70-$7D or $F0-$FF address.");
        return _saveRam[offset % SaveRamByteCount];
    }

    /// <summary>
    /// Writes the currently supported mutable regions: WRAM and SRAM.
    /// </summary>
    public void WriteByte(int address, byte value)
    {
        ValidateAddress(address);
        int bank = SnesAddressMath.BankNumber(address);
        int offset = SnesAddressMath.OffsetInBank(address);

        if (SnesBusLayout.IsWorkRamBank(bank))
        {
            _workRam[(bank - SnesBusLayout.FirstWorkRamBank) * SnesBusLayout.BankBytes + offset] = value;
            return;
        }

        if (SnesBusLayout.IsSystemBank(bank) && offset < SnesBusLayout.WorkRamMirrorEnd)
        {
            _workRam[offset] = value;
            return;
        }

        if (SnesBusLayout.IsSaveRamBank(bank) && offset < SnesBusLayout.RomWindowStart)
        {
            _saveRam[offset % SaveRamByteCount] = value;
            return;
        }

        if (offset >= SnesBusLayout.RomWindowStart)
            throw new InvalidOperationException($"Cannot write cartridge ROM at ${bank:X2}:{offset:X4}.");

        throw new InvalidOperationException(
            $"CPU write ${bank:X2}:{offset:X4} is outside the runtime address map.");
    }

    /// <summary>Rejects values outside the SNES CPU bus's 24-bit address range before decoding bank and offset.</summary>
    private static void ValidateAddress(int address)
    {
        if ((uint)address > SnesBusLayout.MaximumAddress)
            throw new ArgumentOutOfRangeException(nameof(address), address, "SNES CPU address must fit in 24 bits.");
    }
}
