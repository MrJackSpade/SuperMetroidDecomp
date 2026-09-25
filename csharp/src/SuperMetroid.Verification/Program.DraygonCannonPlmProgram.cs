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
        VerifyDraygonCannonVisualInstallation(rom);

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
        var blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x53 * 8] = 0x53;
        blockDefinitions[0x54 * 8] = 0x54;
        var level = CreateRoom(width, 32,
            new ushort[width * 32], new byte[width * 32],
            blockDefinitions: blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var disabled = new List<ushort>();
        var plms = new RoomPlmSystem();
        var entries = DraygonCannonPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmDraygonCannonVisualEntry(
                DraygonCannonPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())).ToArray();
        entries.Single(entry => entry.Id == "right-shield-a").Blocks[0] = 0x0053;
        entries.Single(entry => entry.Id == "left-shield-a").Blocks[0] = 0x0054;
        plms.DraygonCannonVisuals = new RoomPlmDraygonCannonVisualCatalog(entries);
        AssertEqual(4, plms.LoadRoomPopulation(guarded, level, streamer,
            new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Maridia,
            getSamus: () => null,
            isAreaTorizoDefeated: () => false,
            disableDraygonCannon: disabled.Add),
            "four retail cannon headers load with cartridge program lists");

        static void Step(RoomPlmSystem plms, ISnesAddressSpace bus,
            RoomLevelData level, BackgroundTilemapStreamer streamer) =>
            // Keep both upper and lower shield origins inside the visible
            // tilemap ring so their immediate redraws are observable.
            plms.Step(bus, level, streamer, 0, 0x00e0, 0);

        Step(plms, guarded, level, streamer);
        int rightShieldBlock = 18 * width + 2;
        int leftShieldBlock = 15 * width + 29;
        AssertEqual((ushort)0xc514,
            level.GetCollisionBlockByIndex(rightShieldBlock).LevelWord,
            "edited right shield retains native physical collision and tile reference");
        AssertEqual((ushort)0x0053,
            plms.TilemapUpdates.Last(update =>
                update.BlockIndex == rightShieldBlock).TopRow[0],
            "edited right shield reaches the immediate tilemap update");
        AssertEqual((ushort)0x0053,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                rightShieldBlock, 0).TopRow[0],
            "edited right shield survives later room streaming");
        AssertEqual((ushort)0xc114,
            level.GetCollisionBlockByIndex(leftShieldBlock).LevelWord,
            "edited left shield retains native physical collision and tile reference");
        AssertEqual((ushort)0x0054,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                leftShieldBlock, 0).TopRow[0],
            "edited offscreen left shield enters later background streaming");
        AssertTrue(disabled.Contains(DraygonCannonData.UpperLeftDisabledWord),
            "pre-destroyed cannon executes its retail damage-list entry");
        int rightBlock = rightShieldBlock;
        AssertTrue(plms.TryNotifyResidentProjectileHit(rightBlock, 0x0200),
            "Super Missile reaches live right cannon");
        var seenRightDamageWords = new HashSet<ushort>();
        for (int frame = 0; frame < 28; frame++)
        {
            Step(plms, guarded, level, streamer);
            seenRightDamageWords.Add(
                level.GetCollisionBlockByIndex(rightBlock).LevelWord);
        }
        foreach (ushort word in new ushort[] { 0xa580, 0xa581, 0xa582, 0xa583 })
            AssertTrue(seenRightDamageWords.Contains(word),
                $"right cannon damage loop draws compiled frame ${word:X4}");
        AssertTrue(disabled.Contains(DraygonCannonData.LowerLeftDisabledWord),
            "right cannon follows native three-hit threshold to damaged list");
        int leftBlock = leftShieldBlock;
        AssertTrue(plms.TryNotifyResidentProjectileHit(leftBlock, 0x0200),
            "Super Missile reaches live left cannon");
        var seenLeftDamageWords = new HashSet<ushort>();
        for (int frame = 0; frame < 28; frame++)
        {
            Step(plms, guarded, level, streamer);
            seenLeftDamageWords.Add(
                level.GetCollisionBlockByIndex(leftBlock).LevelWord);
        }
        foreach (ushort word in new ushort[] { 0xa180, 0xa181, 0xa182, 0xa183 })
            AssertTrue(seenLeftDamageWords.Contains(word),
                $"left cannon damage loop draws compiled frame ${word:X4}");
        AssertTrue(disabled.Contains(DraygonCannonData.UpperRightDisabledWord),
            "left cannon follows native threshold to damaged list");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail right/left cannon sequences do not reread compiled program or draw bytes");
        Console.WriteLine("  Draygon cannon PLM: both retail programs match ROM and execute through damage without program reads.");
    }

    private sealed class DraygonCannonProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if ((address >= 0x84dcde && address <= 0x84dd26) ||
                (address >= 0x84ddb9 && address <= 0x84de01) ||
                DraygonCannonPlmDrawDefinitions.All.Any(draw =>
                {
                    int start = 0x840000 | draw.Pointer;
                    int size = draw.Runs.Span.ToArray().Sum(run =>
                        4 + run.LevelWords.Length * 2);
                    return address >= start && address < start + size;
                }))
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
