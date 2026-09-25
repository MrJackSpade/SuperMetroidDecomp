using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledDynamicCollectibleGraphics(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount,
            RoomPlmDynamicCollectibleGraphicsDefinitions.All.Length,
            "compiled dynamic collectible graphics count");
        foreach (RoomPlmDynamicCollectibleGraphic graphic in
                 RoomPlmDynamicCollectibleGraphicsDefinitions.All)
        {
            int graphicsBase = 0x890000 | graphic.GraphicsPointer;
            for (int offset = 0; offset < 0x100; offset++)
                AssertEqual(rom.ReadByte(graphicsBase + offset),
                    graphic.Tiles.Span[offset],
                    $"{graphic.Kind} bank-$89 character byte {offset}");

            int kind = (int)graphic.Kind;
            foreach (ushort baseHeader in new ushort[]
                     { RoomPlmHeaders.ExposedEnergyTank,
                       RoomPlmHeaders.ChozoEnergyTank,
                       RoomPlmHeaders.ShotBlockEnergyTank })
            {
                ushort header = checked((ushort)(baseHeader + kind * 4));
                ushort instruction = ReadCollectibleGraphicsWord(rom, header + 2);
                AssertEqual(RoomPlmInstructionCodes.LoadItemGraphics,
                    ReadCollectibleGraphicsWord(rom, instruction),
                    $"{graphic.Kind} header $84:{header:X4} upload instruction");
                AssertEqual(graphic.GraphicsPointer,
                    ReadCollectibleGraphicsWord(rom, instruction + 2),
                    $"{graphic.Kind} header $84:{header:X4} graphics pointer");
                for (int offset = 0; offset < 8; offset++)
                    AssertEqual(rom.ReadByte(0x840000 | (instruction + 4 + offset)),
                        graphic.PaletteOffsets.Span[offset],
                        $"{graphic.Kind} header $84:{header:X4} palette offset {offset}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => RoomPlmDynamicCollectibleGraphicsDefinitions.Get(
                InWorldCollectibleKind.EnergyTank),
            "non-dynamic item cannot request a graphics upload");

        // Retail population $8F:83FE contains a Chozo-orb Bombs item. Ban every
        // compiled graphics payload and every retail item-list upload from the bus,
        // then exercise the real sequential room loader and assert its resulting VRAM.
        var guarded = new RetailCollectibleGraphicsReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(width, 64,
            new ushort[width * 64], new byte[width * 64],
            new ushort[width * 64], new byte[0x400 * 8]);
        var vram = new SnesVram();
        var plms = new RoomPlmSystem();
        plms.LoadRoomPopulation(guarded, level,
            level.CreateBackgroundStreamer(), vram, 0x83fe,
            new Bank80SystemState(), AreaId.Crateria,
            () => new SamusState(), () => false,
            useCompiledRetailPopulation: true);
        AssertTrue(plms.Collectibles.Any(item =>
                item.Kind == InWorldCollectibleKind.Bombs),
            "retail Bombs item remains allocated after compiled graphics upload");
        ReadOnlySpan<byte> expected =
            RoomPlmDynamicCollectibleGraphicsDefinitions.Get(
                InWorldCollectibleKind.Bombs).Tiles.Span;
        for (int offset = 0; offset < expected.Length; offset++)
            AssertEqual(expected[offset], vram.ReadByte(0x3e00 * 2 + offset),
                $"retail Bombs item VRAM character byte {offset}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail collectible loader does not reread compiled character or palette bytes");
        VerifyInstalledDynamicCollectibleArt(rom);
    }

    private static ushort ReadCollectibleGraphicsWord(
        ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(0x840000 | address) |
        bus.ReadByte(0x840000 | (address + 1)) << 8));

    private sealed class RetailCollectibleGraphicsReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (RoomPlmDynamicCollectibleGraphic graphic in
                     RoomPlmDynamicCollectibleGraphicsDefinitions.All)
            {
                int first = 0x890000 | graphic.GraphicsPointer;
                if (address >= first && address < first + graphic.Tiles.Length)
                    return Reject(address);
                int kind = (int)graphic.Kind;
                foreach (ushort baseHeader in new ushort[]
                         { RoomPlmHeaders.ExposedEnergyTank,
                           RoomPlmHeaders.ChozoEnergyTank,
                           RoomPlmHeaders.ShotBlockEnergyTank })
                {
                    ushort header = checked((ushort)(baseHeader + kind * 4));
                    ushort instruction = ReadCollectibleGraphicsWord(source, header + 2);
                    int instructionAddress = 0x840000 | instruction;
                    if (address >= instructionAddress &&
                        address < instructionAddress + 12)
                        return Reject(address);
                }
            }
            return source.ReadByte(address);
        }

        private byte Reject(int address)
        {
            ForbiddenReadAttempts++;
            throw new InvalidOperationException(
                $"Retail collectible reread compiled graphics byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
