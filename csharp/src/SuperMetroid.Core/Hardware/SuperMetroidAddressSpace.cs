namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Cartridge ROM, work RAM, and save RAM portions of Super Metroid's 24-bit CPU address
/// space. Hardware registers are intentionally left unmapped until their behavior is ported.
/// </summary>
/// <remarks>
/// This mapper follows the native decompilation's <c>RomPtr</c> expression:
/// <c>(((bank &lt;&lt; 15) | (offset &amp; $7FFF)) &amp; $3FFFFF)</c>. The retail image contains
/// 3 MiB inside that possible 4 MiB LoROM window. An address falling in the unused final
/// MiB throws rather than reading invented padding.
/// </remarks>
public sealed class SuperMetroidAddressSpace : ISnesAddressSpace
{
    /// <summary>Super Metroid's unheadered retail ROM size.</summary>
    public const int RetailRomByteCount = 0x300000;

    /// <summary>Two complete 64 KiB WRAM banks, <c>$7E</c> and <c>$7F</c>.</summary>
    public const int WorkRamByteCount = 0x20000;

    /// <summary>8 KiB of battery-backed SRAM used by three save slots and metadata.</summary>
    public const int SaveRamByteCount = 0x2000;

    private readonly byte[] _rom;
    private readonly byte[] _workRam = new byte[WorkRamByteCount];
    private readonly byte[] _saveRam = new byte[SaveRamByteCount];

    /// <summary>
    /// Creates a mapper around an unheadered ROM image. The general constructor accepts
    /// synthetic LoROM-sized data for tests; use <see cref="LoadRetailRom"/> for the strict
    /// title/size checks appropriate to the game runtime.
    /// </summary>
    public SuperMetroidAddressSpace(ReadOnlySpan<byte> unheaderedRom)
    {
        if (unheaderedRom.IsEmpty || unheaderedRom.Length > 0x400000 || (unheaderedRom.Length & 0x7fff) != 0)
        {
            throw new ArgumentException(
                "ROM must contain one to 128 complete 32 KiB LoROM banks.",
                nameof(unheaderedRom));
        }

        // Own a stable copy. Asset tools often pass memory backed by temporary file buffers;
        // retaining an external span would make later DMA behavior depend on its lifetime.
        _rom = unheaderedRom.ToArray();
    }

    /// <summary>Read-only cartridge bytes for disassembly and debugger inspection.</summary>
    public ReadOnlySpan<byte> Rom => _rom;

    /// <summary>Mutable physical WRAM used by translated routines and save-state tools.</summary>
    public Span<byte> WorkRam => _workRam;

    /// <summary>Mutable battery-backed SRAM; persistence remains an explicit host concern.</summary>
    public Span<byte> SaveRam => _saveRam;

    /// <summary>
    /// Loads this project's retail Super Metroid ROM, accepting either an unheadered 3 MiB
    /// image or the same bytes preceded by a 512-byte copier header.
    /// </summary>
    public static SuperMetroidAddressSpace LoadRetailRom(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] fileBytes = File.ReadAllBytes(path);

        ReadOnlySpan<byte> romBytes;
        if (fileBytes.Length == RetailRomByteCount)
        {
            romBytes = fileBytes;
        }
        else if (fileBytes.Length == RetailRomByteCount + 512)
        {
            // Old cartridge copiers commonly prepended a 512-byte metadata/header block.
            // It is not visible on the SNES bus and must not participate in LoROM offsets.
            romBytes = fileBytes.AsSpan(512);
        }
        else
        {
            throw new InvalidDataException(
                $"Expected a ${RetailRomByteCount:X} byte retail ROM (optionally plus a 512-byte copier header), " +
                $"but '{path}' contains ${fileBytes.Length:X} bytes.");
        }

        // The 21-byte internal title resides at unheadered file offset $7FC0. Only test the
        // meaningful prefix because the remaining header title bytes are space-padded.
        // The header stores title case exactly as "Super Metroid", followed by spaces.
        // SNES title fields are byte strings, so keep this comparison case-sensitive.
        ReadOnlySpan<byte> expectedTitle = "Super Metroid"u8;
        if (!romBytes.Slice(0x7fc0, expectedTitle.Length).SequenceEqual(expectedTitle))
            throw new InvalidDataException($"'{path}' does not contain the expected SUPER METROID LoROM header title.");

        return new SuperMetroidAddressSpace(romBytes);
    }

    /// <inheritdoc />
    public byte ReadByte(int address)
    {
        ValidateAddress(address);
        int bank = address >> 16;
        int offset = address & 0xffff;

        if (bank is 0x7e or 0x7f)
        {
            // Banks $7E/$7F expose the complete 128 KiB physical work RAM linearly.
            return _workRam[(bank - 0x7e) * 0x10000 + offset];
        }

        if (IsSystemBank(bank) && offset < 0x2000)
        {
            // Banks $00-$3F and $80-$BF mirror the first 8 KiB of bank-$7E WRAM. Most bank
            // $80 routines use this direct-page/absolute mirror for shared engine state.
            return _workRam[offset];
        }

        if (IsSaveRamBank(bank) && offset < 0x8000)
        {
            // The physical SRAM is only 8 KiB, so its larger LoROM windows repeat every
            // $2000 bytes and across each mapped bank.
            return _saveRam[offset & 0x1fff];
        }

        if (offset >= 0x8000)
        {
            int romOffset = ToRomOffset(address);
            if ((uint)romOffset >= _rom.Length)
            {
                throw new InvalidOperationException(
                    $"CPU address ${bank:X2}:{offset:X4} maps to unpopulated ROM offset ${romOffset:X6}.");
            }

            return _rom[romOffset];
        }

        // This range contains PPU/APU/CPU registers, expansion space, and other mappings.
        // Returning zero would hide every missing hardware implementation behind bad data.
        throw new NotSupportedException($"CPU read ${bank:X2}:{offset:X4} is not mapped by the current runtime.");
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

        if (IsSystemBank(bank) && offset < 0x2000)
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

        throw new NotSupportedException($"CPU write ${bank:X2}:{offset:X4} is not mapped by the current runtime.");
    }

    /// <summary>
    /// Converts a ROM-window CPU address with the exact four-megabyte mask used by the
    /// native decompilation. Callers still need to ensure the result is populated.
    /// </summary>
    public static int ToRomOffset(int address)
    {
        ValidateAddress(address);
        if ((address & 0x8000) == 0)
            throw new ArgumentOutOfRangeException(nameof(address), address, "Address is outside the upper LoROM window.");

        int bank = address >> 16;
        return ((bank << 15) | (address & 0x7fff)) & 0x3f_ffff;
    }

    private static bool IsSystemBank(int bank) => bank <= 0x3f || bank is >= 0x80 and <= 0xbf;

    private static bool IsSaveRamBank(int bank) => bank is >= 0x70 and <= 0x7d or >= 0xf0 and <= 0xff;

    private static void ValidateAddress(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address), address, "SNES CPU address must fit in 24 bits.");
    }
}
