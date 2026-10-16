using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The live memory Samus's cartridge tables reach when a native pointer, index or glitched
/// selector leaves the authored data (#629). The lower half of a system bank ($90, $91, $92,
/// $9B) is the low-WRAM mirror and the unconnected I/O windows, not cartridge data, so these
/// exceptional cartridge paths read mutable state with the exact native access pattern.
/// </summary>
[CpuBusBoundary("domain view: Samus table over-reads that reach live memory")]
internal readonly struct SamusOverreadMemory(ISnesAddressSpace bus)
{
    /// <summary>Requires mutable console memory for a bank-low-half access, naming the native reader on failure.</summary>
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
        int bank = SnesAddressMath.BankNumber(address);
        int offset = SnesAddressMath.OffsetInBank(address);
        if ((bank & 1 << 6) == 0 &&
            (offset >= SnesBusLayout.ReservedBBusStart &&
             offset <= SnesBusLayout.ReservedBBusEnd ||
             offset >= SnesBusLayout.UnpopulatedExpansionStart &&
             offset <= SnesBusLayout.UnpopulatedExpansionEnd))
            return memoryDataRegister;

        return SnesLiveDataRead.TryRead(bus, address, "Trail coordinate", out byte value)
            ? value
            : throw new InvalidOperationException($"Trail coordinate ${address:X6} requires a compiled cartridge definition.");
    }

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
