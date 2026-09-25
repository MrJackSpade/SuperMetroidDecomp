using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDraygonCannonPlmProgram()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        foreach ((ushort first, ushort last) in new[]
        {
            (DraygonCannonPlmProgramDefinitions.RightStart,
                DraygonCannonPlmProgramDefinitions.RightEnd),
            (DraygonCannonPlmProgramDefinitions.LeftStart,
                DraygonCannonPlmProgramDefinitions.LeftEnd),
        })
        {
            for (int address = first; address <= last; address++)
            {
                AssertTrue(DraygonCannonPlmProgramDefinitions.TryReadMechanicsByte(
                    checked((ushort)address), out byte compiled),
                    $"Draygon cannon claims program byte $84:{address:X4}");
                AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                    $"Draygon cannon program byte $84:{address:X4} matches ROM");
                if (address == last)
                    continue;
                AssertTrue(DraygonCannonPlmProgramDefinitions.TryReadMechanicsWord(
                    checked((ushort)address), out ushort compiledWord),
                    $"Draygon cannon claims program word $84:{address:X4}");
                ushort native = (ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8);
                AssertEqual(native, compiledWord,
                    $"Draygon cannon program word $84:{address:X4} matches ROM");
            }
        }
        AssertTrue(!DraygonCannonPlmProgramDefinitions.TryReadMechanicsByte(0xdd27, out _),
            "unused diagonal cannon list is not claimed");
        AssertTrue(!DraygonCannonPlmProgramDefinitions.TryReadMechanicsWord(0xdd26, out _),
            "right list refuses a word crossing into diagonal data");
        AssertTrue(!DraygonCannonPlmProgramDefinitions.TryReadMechanicsByte(0xde02, out _),
            "left list does not claim adjacent diagonal data");

        var source = new TestAddressSpace();
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        source.WriteBytes(0x848000, bank84);
        source.WriteBytes(0x8f9000,
        [
            0x65, 0xdf, 2, 11, 0x02, 0x88,
            0x59, 0xdf, 2, 18, 0x04, 0x88,
            0x71, 0xdf, 29, 15, 0x06, 0x88,
            0x71, 0xdf, 29, 21, 0x08, 0x88,
            0, 0,
        ]);
        var guarded = new DraygonCannonProgramReadGuard(source);
        const int width = 32;
        var level = CreateRoom(width, 32,
            new ushort[width * 32], new byte[width * 32],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var disabled = new List<ushort>();
        var plms = new RoomPlmSystem();
        AssertEqual(4, plms.LoadRoomPopulation(guarded, level, streamer,
            new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Maridia,
            getSamus: () => null,
            isAreaTorizoDefeated: () => false,
            disableDraygonCannon: disabled.Add),
            "four retail cannon headers load with cartridge program lists");

        static void Step(RoomPlmSystem plms, ISnesAddressSpace bus,
            RoomLevelData level, BackgroundTilemapStreamer streamer) =>
            plms.Step(bus, level, streamer, 0, 0, 0);

        Step(plms, guarded, level, streamer);
        AssertTrue(disabled.Contains(DraygonCannonData.UpperLeftDisabledWord),
            "pre-destroyed cannon executes its retail damage-list entry");
        int rightBlock = 18 * width + 2;
        AssertTrue(plms.TryNotifyResidentProjectileHit(rightBlock, 0x0200),
            "Super Missile reaches live right cannon");
        for (int frame = 0; frame < 12; frame++)
            Step(plms, guarded, level, streamer);
        AssertTrue(disabled.Contains(DraygonCannonData.LowerLeftDisabledWord),
            "right cannon follows native three-hit threshold to damaged list");
        int leftBlock = 15 * width + 29;
        AssertTrue(plms.TryNotifyResidentProjectileHit(leftBlock, 0x0200),
            "Super Missile reaches live left cannon");
        for (int frame = 0; frame < 12; frame++)
            Step(plms, guarded, level, streamer);
        AssertTrue(disabled.Contains(DraygonCannonData.UpperRightDisabledWord),
            "left cannon follows native threshold to damaged list");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail right/left cannon sequences do not reread compiled program bytes");
        Console.WriteLine("  Draygon cannon PLM: both retail programs match ROM and execute through damage without program reads.");
    }

    private sealed class DraygonCannonProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if ((address >= 0x84dcde && address <= 0x84dd26) ||
                (address >= 0x84ddb9 && address <= 0x84de01))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Draygon cannon reread compiled program byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
