namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The mutable work RAM portion of Super Metroid's 24-bit CPU address space. Cartridge ROM
/// belongs to the asset importer, and saves to <see cref="Saves.SaveSlotStore"/>, never to the
/// gameplay address space.
/// </summary>
/// <remarks>
/// Hardware registers, SRAM and cartridge windows are intentionally unmapped.
/// </remarks>
[CpuBusBoundary("CPU-bus implementation: maps WRAM banks and low-window mirrors.")]
public class SuperMetroidAddressSpace : ISnesAddressSpace, ISnesMutableMemory
{
    /// <summary>Two complete 64 KiB WRAM banks, <c>$7E</c> and <c>$7F</c>.</summary>
    public const int WorkRamByteCount = 2 * SnesBusLayout.BankBytes;

    /// <summary>Backing bytes for both full WRAM banks and their low-window system-bank mirrors.</summary>
    internal readonly byte[] WorkRam = new byte[WorkRamByteCount];

    /// <summary>
    /// Creates the mutable WRAM portion of the runtime address space without a cartridge
    /// payload. It fails loudly if code accidentally attempts to read a cartridge address.
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
            return WorkRam[(bank - SnesBusLayout.FirstWorkRamBank) * SnesBusLayout.BankBytes + offset];
        if (SnesBusLayout.IsSystemBank(bank) && offset < SnesBusLayout.WorkRamMirrorEnd)
            return WorkRam[offset];
        throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
            "WRAM reads require bank $7E/$7F or a low-window system-bank mirror.");
    }

    /// <summary>Rejects values outside the SNES CPU bus's 24-bit address range before decoding bank and offset.</summary>
    internal static void ValidateAddress(int address)
    {
        if ((uint)address > SnesBusLayout.MaximumAddress)
            throw new ArgumentOutOfRangeException(nameof(address), address, "SNES CPU address must fit in 24 bits.");
    }
}
