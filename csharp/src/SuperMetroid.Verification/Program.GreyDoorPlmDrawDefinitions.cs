using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyGreyDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        VerifyGreyDoorProgramDefinitions(rom);
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            GreyDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmGreyDoorVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmGreyDoorVisualEntry(
                GreyDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())).ToArray();
        RoomPlmGreyDoorVisualCatalog stock = RoomPlmGreyDoorVisualCatalog.Stock();
        RoomPlmGreyDoorVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "grey-right-frame-0");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmGreyDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        AssertEqual((ushort)0x0053, edited.GetWord(0xa6d7, 0),
            "grey-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(0xa6d7, 0),
            "stock grey-door visual retains native tile choice");
        AssertEqual(20, lists.Length,
            "four grey-cap orientations each have four frames plus four shared clear frames");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            AssertEqual(1, list.Runs.Length,
                $"grey-door cap ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadWord(rom, source),
                $"grey-door cap ${list.Pointer:X4} direction/count matches ROM");
            AssertEqual(4, run.LevelWords.Length,
                $"grey-door cap ${list.Pointer:X4} has four physical words");
            for (int block = 0; block < 4; block++)
                AssertEqual(run.LevelWords.Span[block],
                    ReadWord(rom, source + 2 + block * 2),
                    $"grey-door cap ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0, ReadWord(rom, source + 10),
                $"grey-door cap ${list.Pointer:X4} has a zero offset terminator");
        }

        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = rom.ReadByte(0x848000 + offset);
        ushort[] headers =
        [
            RoomPlmHeaders.BombTorizoGreyDoor,
            RoomPlmHeaders.GreyDoorFacingLeft,
            RoomPlmHeaders.GreyDoorFacingRight,
            RoomPlmHeaders.GreyDoorFacingUp,
            RoomPlmHeaders.GreyDoorFacingDown,
        ];
        foreach (ushort header in headers)
        {
            var bus = new TestAddressSpace();
            bus.WriteBytes(0x848000, bank84);
            const ushort population = 0x9000;
            bus.WriteBytes(0x8f0000 | population,
            [
                unchecked((byte)header), unchecked((byte)(header >> 8)),
                4, 4, 0, 0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            byte[] blockDefinitions = new byte[0x400 * 8];
            blockDefinitions[(originalVisual & 0x3ff) * 8] = 0xae;
            blockDefinitions[0x53 * 8] = 0x53;
            var level = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], blockDefinitions);
            var plms = new RoomPlmSystem
            {
                GreyDoorVisuals = header == RoomPlmHeaders.BombTorizoGreyDoor
                    ? edited : stock,
            };
            var system = new Bank80SystemState();
            var guarded = new GreyDoorDrawReadGuard(bus, lists);
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
                    new SnesVram(), population, system,
                    AreaId.Crateria, () => new SamusState(), () => false),
                $"resident grey-door header ${header:X4} loads");
            ushort initial = ReadWord(rom, 0x840000 | (header + 2));
            ushort firstDraw = ReadWord(rom, 0x840000 | (initial + 12));
            AssertTrue(GreyDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"resident grey-door header ${header:X4} selects compiled art");
            plms.Step(guarded, level, streamer, 0, 0, 0);
            bool vertical = (selected.Runs.Span[0].DirectionAndCount & 0x8000) != 0;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).LevelWord,
                    $"resident grey-door ${header:X4} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident grey-door ${header:X4} avoids draw payload ROM reads");
            if (header == RoomPlmHeaders.BombTorizoGreyDoor)
            {
                AssertEqual((ushort)0x0053, plms.TilemapUpdates[0].TopRow[0],
                    "edited Bomb Torizo grey cap reaches immediate redraw");
                AssertEqual((ushort)0x0053,
                    level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
                    "edited grey cap survives later camera streaming");
            }
            else
            {
                var enteringDoor = new CartridgeDoorHeader(
                    Pointer: 0,
                    DestinationRoomPointer: 0,
                    BitFlags: 0,
                    Orientation: 5,
                    PlmX: 4,
                    PlmY: 4,
                    DestinationScreenX: 0,
                    DestinationScreenY: 0,
                    SamusDistance: 0,
                    SetupCodePointer: 0);
                AssertTrue(plms.TrySpawnDoorClosingPlm(guarded, level, enteringDoor,
                        system),
                    $"resident grey door ${header:X4} selects its closing list");
                for (int frame = 0; frame < 20 &&
                     plms.GreyDoors.Single().Phase == GreyDoorPhase.Closing; frame++)
                    plms.Step(guarded, level, streamer, 0, 0, 0);
                AssertEqual(GreyDoorPhase.Locked, plms.GreyDoors.Single().Phase,
                    $"resident grey door ${header:X4} returns to locked owner");

                // The default room argument selects the boss-defeated condition. A
                // closed door must flash only after the cartridge event bit changes.
                system.SetBossBits(AreaId.Crateria, BossBits.AreaBoss);
                plms.Step(guarded, level, streamer, 0, 0, 0);
                AssertEqual(GreyDoorPhase.Flashing, plms.GreyDoors.Single().Phase,
                    $"resident grey door ${header:X4} unlocks after boss event");
                AssertTrue(plms.TryNotifyColoredDoorHit(origin,
                        new SamusProjectileTypeWord(0)),
                    $"resident grey door ${header:X4} accepts a beam hit");
                plms.Step(guarded, level, streamer, 0, 0, 0);
                AssertEqual(GreyDoorPhase.Opening, plms.GreyDoors.Single().Phase,
                    $"resident grey door ${header:X4} enters opening list");
                for (int frame = 0; frame < 128 && plms.ActiveCount != 0; frame++)
                    plms.Step(guarded, level, streamer, 0, 0, 0);
                AssertEqual(0, plms.ActiveCount,
                    $"resident grey door ${header:X4} completes and deletes");
                AssertTrue(system.HasOpenedDoorBit(0),
                    $"resident grey door ${header:X4} persists opened bit");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"resident grey door ${header:X4} never rereads program or draw data");

                var reopenedLevel = new RoomLevelData(width, width,
                    new ushort[width * width], new byte[width * width],
                    new ushort[width * width], blockDefinitions);
                BackgroundTilemapStreamer reopenedStreamer =
                    reopenedLevel.CreateBackgroundStreamer();
                var reopened = new RoomPlmSystem { GreyDoorVisuals = stock };
                AssertEqual(1, reopened.LoadRoomPopulation(guarded, reopenedLevel,
                        reopenedStreamer, new SnesVram(), population, system,
                        AreaId.Crateria, () => new SamusState(), () => false),
                    $"opened grey door ${header:X4} reloads");
                reopened.Step(guarded, reopenedLevel, reopenedStreamer, 0, 0, 0);
                AssertEqual(0, reopened.ActiveCount,
                    $"opened grey door ${header:X4} converts to blue cap");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"opened grey door ${header:X4} avoids program/draw ROM reads");
            }
        }
        AssertThrows<InvalidDataException>(
            () => new RoomPlmGreyDoorVisualCatalog(entries.Skip(1)),
            "grey-door catalog rejects missing frames");
        editedFrame.Blocks[0] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmGreyDoorVisualCatalog(entries),
            "grey-door catalog rejects collision bits in visual words");
        editedFrame.Blocks[0] = originalVisual;
        VerifySharedDoorClearVisual(rom, entries, lists);
        VerifyGreyDoorVisualInstallation(rom);
        Console.WriteLine(
            "  Grey doors: 420 compiled program bytes, four close/unlock/open/reload paths, 20 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    private static void VerifyGreyDoorProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int address = GreyDoorPlmProgramDefinitions.FirstAddress;
             address <= GreyDoorPlmProgramDefinitions.LastAddress; address++)
        {
            AssertTrue(GreyDoorPlmProgramDefinitions.TryReadMechanicsByte(
                    checked((ushort)address), out byte compiled),
                $"grey-door program claims byte $84:{address:X4}");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"grey-door program byte $84:{address:X4} matches ROM");
            if (address == GreyDoorPlmProgramDefinitions.LastAddress)
                continue;
            AssertTrue(GreyDoorPlmProgramDefinitions.TryReadMechanicsWord(
                    checked((ushort)address), out ushort compiledWord),
                $"grey-door program claims word $84:{address:X4}");
            ushort native = (ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8);
            AssertEqual(native, compiledWord,
                $"grey-door program word $84:{address:X4} matches ROM");
        }
        AssertTrue(!GreyDoorPlmProgramDefinitions.TryReadMechanicsByte(0xbe58, out _),
            "ordinary grey-door program does not claim preceding condition table");
        AssertTrue(!GreyDoorPlmProgramDefinitions.TryReadMechanicsByte(0xbffd, out _),
            "ordinary grey-door program does not claim following yellow door");
        AssertTrue(!GreyDoorPlmProgramDefinitions.TryReadMechanicsWord(0xbffc, out _),
            "ordinary grey-door program refuses a cross-family word");
    }

    private static void VerifySharedDoorClearVisual(
        SuperMetroidAddressSpace rom,
        RoomPlmGreyDoorVisualEntry[] entries,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists)
    {
        RoomPlmGreyDoorVisualEntry clear = entries.Single(entry =>
            entry.Id == "clear-right");
        ushort original = clear.Blocks[0];
        clear.Blocks[0] = 0x0054;
        var edited = new RoomPlmGreyDoorVisualCatalog(entries);
        clear.Blocks[0] = original;

        const int width = 16;
        const int origin = 4 + 4 * width;
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x54 * 8] = 0x54;
        var level = new RoomLevelData(width, width,
            new ushort[width * width], new byte[width * width],
            new ushort[width * width], blockDefinitions);
        var plms = new RoomPlmSystem { GreyDoorVisuals = edited };
        AssertTrue(plms.TrySpawnBlueDoorOpening(level, origin,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                new SamusProjectileTypeWord(0)),
            "shared clear visual test allocates a real blue-door opening actor");
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var guarded = new GreyDoorDrawReadGuard(rom, lists);
        int frame = 0;
        while (frame++ < 200 &&
            level.GetCollisionBlockByIndex(origin).LevelWord != 0x0482)
            plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertTrue(frame <= 200,
            "blue-door opening reaches the shared right-facing clear draw");
        AssertEqual((ushort)0x0482,
            level.GetCollisionBlockByIndex(origin).LevelWord,
            "shared clear art leaves the native physical air word intact");
        AssertEqual((ushort)0x0054,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
            "edited shared clear tile reaches blue-door camera streaming");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "blue-door shared clear draw does not reread bank-$84 payload bytes");
    }

    private static void VerifyGreyDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "grey-door-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Grey-door test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmGreyDoorVisualFiles.Extract(rom,
                installation.RoomPlmGreyDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmGreyDoorVisualFiles.ValidateStock(
                installation.RoomPlmGreyDoorVisualDirectory);
            ushort stock = installation.LoadRoomPlmGreyDoorVisuals().GetWord(0xa6d7, 0);
            string stockPath = Path.Combine(installation.RoomPlmGreyDoorVisualDirectory,
                RoomPlmGreyDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted grey-door JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "grey-right-frame-0")!;
            frame["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmGreyDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmGreyDoorVisualOverrideDirectory,
                RoomPlmGreyDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmGreyDoorVisuals().GetWord(0xa6d7, 0),
                "installed grey-door override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmGreyDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmGreyDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmGreyDoorVisualOverrideDirectory)
                    .GetWord(0xa6d7, 0),
                "grey-door override survives stock replacement");

            frame["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGreyDoorVisuals(),
                "invalid grey-door override fails loudly");
            frame["blocks"]![0] = stock;
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGreyDoorVisuals(),
                "tampered grey-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class GreyDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= 0x84be59 && address <= 0x84bffc)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Resident grey door reread compiled instruction ${address:X6}.");
            }
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                if (address >= first &&
                    address < first + GreyDoorPlmDrawDefinitions.DrawListBytes)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Resident grey door reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
