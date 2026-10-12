using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The live memory Samus's cartridge tables reach when a native pointer, index or glitched
/// selector leaves the authored data (#629). The lower half of a system bank ($90, $91, $92,
/// $9B) is the low-WRAM mirror and the unconnected I/O windows, not cartridge data, so these
/// exceptional cartridge paths read mutable state with the exact native access pattern.
/// </summary>
[CpuBusBoundary("domain view: Samus table over-reads that reach live memory")]
internal readonly struct SamusOverreadMemory(ISnesAddressSpace bus)
{
    private ISnesMutableMemory WorkRam(string reader) =>
        bus as ISnesMutableMemory ?? throw new InvalidOperationException($"{reader} requires WRAM access.");

    /// <summary>
    /// A pose animation-list byte at bank-$91 low-half <paramref name="offset"/> (the delay-table
    /// overread of invalid poses, or running-cadence stage five's <c>$91:0303</c>).
    /// </summary>
    internal byte AnimationListByte(ushort offset) =>
        WorkRam("Samus's mutable bank-$91 animation-list alias").ReadWorkRamByte(SamusMovementRomData.Banks.Pose | offset);

    /// <summary>
    /// Atmospheric effect type two's sprite attributes for <paramref name="frame"/>: its native
    /// list pointer is zero, so bank $90 indexes the live WRAM mirror.
    /// </summary>
    internal ushort AtmosphericTypeTwoAttributes(byte frame)
    {
        ISnesMutableMemory memory = WorkRam("Atmospheric type two's mirrored WRAM");
        int source = SamusMovementRomData.Banks.Movement | (frame * sizeof(ushort));
        return memory.ReadWorkRamWord(source);
    }

    /// <summary>
    /// The six words of a horizontal speed entry at a bank-$90 low-half <paramref name="address"/>,
    /// each read low byte first with its high byte wrapping within the bank.
    /// </summary>
    internal SpeedTableEntry HorizontalSpeedEntry(int address)
    {
        ISnesMutableMemory memory = WorkRam("A wrapped speed table's live WRAM");
        return new SpeedTableEntry(
            memory.ReadWorkRamWordFixedBank(address + 0),
            memory.ReadWorkRamWordFixedBank(address + 2),
            memory.ReadWorkRamWordFixedBank(address + 4),
            memory.ReadWorkRamWordFixedBank(address + 6),
            memory.ReadWorkRamWordFixedBank(address + 8),
            memory.ReadWorkRamWordFixedBank(address + 10));
    }

    /// <summary>A projectile-trail program word at a bank-$90 low-half <paramref name="source"/>.</summary>
    internal ushort TrailProgramWord(SnesAddress source)
    {
        ISnesMutableMemory memory = WorkRam("Projectile trail low-bank alias's mutable console memory");
        return (ushort)(SnesLittleEndian.Word(memory.ReadWorkRamByte((int)source), memory.ReadWorkRamByte((int)source.AddWithinBank(1))));
    }

    /// <summary>A projectile-trail direction byte in bank $9B's lower half: live memory or a modeled peripheral.</summary>
    internal byte TrailDirectionByte(int address) =>
        SnesLiveDataRead.TryRead(bus, address, "Trail direction", out byte value)
            ? value
            : throw new InvalidDataException($"Trail direction ${address:X6} requires a compiled cartridge definition.");

    /// <summary>
    /// A trail-coordinate absolute-indexed operand byte outside compiled bank-$9B data. The proven
    /// undriven B-bus and expansion windows return <paramref name="memoryDataRegister"/>.
    /// </summary>
    internal byte TrailCoordinateOperandByte(int address, byte memoryDataRegister)
    {
        int bank = address >> 16;
        int offset = address & 0xffff;
        if ((bank & 0x40) == 0 &&
            (offset >= SnesCpuOpenBusWindows.ReservedBBusStart &&
             offset <= SnesCpuOpenBusWindows.ReservedBBusEnd ||
             offset >= SnesCpuOpenBusWindows.UnpopulatedExpansionStart &&
             offset <= SnesCpuOpenBusWindows.UnpopulatedExpansionEnd))
            return memoryDataRegister;

        return SnesLiveDataRead.TryRead(bus, address, "Trail coordinate", out byte value)
            ? value
            : throw new InvalidOperationException($"Trail coordinate ${address:X6} requires a compiled cartridge definition.");
    }

    /// <summary>
    /// Color word <paramref name="index"/> of the Speed Booster palette whose overrun pointer lands
    /// in the expansion window: the native <c>LDA $0000,X</c> under bank $9B with its operand-driven bus value.
    /// </summary>
    internal ushort SpeedBoosterExpansionWord(int index, ushort pointer) =>
        SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus,
            SnesLittleEndian.BankByte(SamusPaletteRomData.Banks.Palette), (ushort)(index * 2), pointer);

    /// <summary>Chainsaw beam palette word <paramref name="color"/>, read through its glitched <c>[$00],Y</c> pointer.</summary>
    internal ushort ChainsawPaletteWord(ushort color) =>
        SnesIndirectLongDataRead.ReadWord(bus, SnesLittleEndian.BankByte(SamusMovementRomData.Banks.Movement),
            ChainsawBeamGraphicsDefinitions.PalettePointer, (ushort)(color * 2),
            ChainsawBeamGraphicsDefinitions.InstructionBytes, ChainsawBeamGraphicsDefinitions.InstructionAddress);

    /// <summary>SpaceTime beam palette word <paramref name="color"/>, read through its glitched pointer into live WRAM.</summary>
    internal ushort SpacetimePaletteWord(ushort color) =>
        SnesIndirectLongDataRead.ReadWord(bus, SnesLittleEndian.BankByte(SamusMovementRomData.Banks.Movement),
            SpacetimeBeamGraphicsDefinitions.PalettePointer, (ushort)(color * 2));

    /// <summary>
    /// The spritemap a zero entry of the installed Samus pointer table selects: the native
    /// bank-local dereference lands in the WRAM mirror at <c>$92:0000</c>. Each entry is an
    /// X word, a Y byte and an attribute word, five bytes apart, walked within the bank.
    /// </summary>
    internal static SamusZeroSpritemapEntry[] ZeroPointerSpritemap(ISnesMutableMemory memory)
    {
        Ensure.NotNull(memory);
        const int spritemapAddress = SamusRenderingRomData.Banks.GraphicsDefinitions;
        ushort entryCount = memory.ReadWorkRamWordFixedBank(spritemapAddress);
        var entries = new SamusZeroSpritemapEntry[entryCount];
        int entryAddress = SnesAddressMath.AddWithinBank(spritemapAddress, 2);
        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            ushort x = memory.ReadWorkRamWordFixedBank(entryAddress);
            byte y = memory.ReadWorkRamByte(SnesAddressMath.AddWithinBank(entryAddress, 2));
            ushort attributes = memory.ReadWorkRamWordFixedBank(SnesAddressMath.AddWithinBank(entryAddress, 3));
            entries[entryIndex] = new SamusZeroSpritemapEntry(x, y, attributes);
            entryAddress = SnesAddressMath.AddWithinBank(entryAddress, 5);
        }
        return entries;
    }
}

/// <summary>One part of the live zero-pointer Samus spritemap: encoded X word, Y byte and OAM attributes.</summary>
internal readonly record struct SamusZeroSpritemapEntry(ushort EncodedX, byte EncodedY, ushort Attributes);
