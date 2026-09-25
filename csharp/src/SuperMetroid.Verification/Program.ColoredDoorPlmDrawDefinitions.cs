using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyColoredDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        VerifyColoredDoorProgramDefinitions(rom);
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            ColoredDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmColoredDoorVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmColoredDoorVisualEntry(
                ColoredDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())).ToArray();
        RoomPlmColoredDoorVisualCatalog stock = RoomPlmColoredDoorVisualCatalog.Stock();
        RoomPlmColoredDoorVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "green-left-frame-0");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmColoredDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        AssertEqual((ushort)0x0053, edited.GetWord(0xa827, 0),
            "colored-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(0xa827, 0),
            "stock colored-door visual retains native tile choice");
        AssertEqual(48, lists.Length,
            "three colored-door families each have four orientations and four frames");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            AssertEqual(1, list.Runs.Length,
                $"colored cap ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadWord(rom, source),
                $"colored cap ${list.Pointer:X4} direction/count matches ROM");
            AssertEqual(4, run.LevelWords.Length,
                $"colored cap ${list.Pointer:X4} has four physical words");
            for (int block = 0; block < 4; block++)
                AssertEqual(run.LevelWords.Span[block],
                    ReadWord(rom, source + 2 + block * 2),
                    $"colored cap ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0, ReadWord(rom, source + 10),
                $"colored cap ${list.Pointer:X4} has a zero offset terminator");
        }

        // A synthetic population selects each real resident header. Only the bank-$8F
        // population is synthetic; setup and first-draw instruction bytes are copied
        // from the pinned bank-$84 cartridge. Blocking all authored draw payloads
        // proves that the actual PLM handler uses the compiled definitions.
        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = rom.ReadByte(0x848000 + offset);
        ushort[] headers =
        [
            RoomPlmHeaders.YellowDoorFacingLeft,
            RoomPlmHeaders.YellowDoorFacingRight,
            RoomPlmHeaders.YellowDoorFacingUp,
            RoomPlmHeaders.YellowDoorFacingDown,
            RoomPlmHeaders.GreenDoorFacingLeft,
            RoomPlmHeaders.GreenDoorFacingRight,
            RoomPlmHeaders.GreenDoorFacingUp,
            RoomPlmHeaders.GreenDoorFacingDown,
            RoomPlmHeaders.RedDoorFacingLeft,
            RoomPlmHeaders.RedDoorFacingRight,
            RoomPlmHeaders.RedDoorFacingUp,
            RoomPlmHeaders.RedDoorFacingDown,
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
            blockDefinitions[originalVisual * 8] = 0x04;
            blockDefinitions[0x53 * 8] = 0x53;
            var level = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], blockDefinitions);
            var plms = new RoomPlmSystem
            {
                ColoredDoorVisuals = header == RoomPlmHeaders.GreenDoorFacingLeft
                    ? edited : stock,
            };
            var system = new Bank80SystemState();
            var guarded = new ColoredDoorDrawReadGuard(bus, lists);
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            AssertEqual(1, plms.LoadRoomPopulation(guarded, level,
                    streamer, new SnesVram(), population,
                    system, AreaId.Crateria,
                    () => new SamusState(), () => false),
                $"resident colored-door header ${header:X4} loads");
            ushort initial = ReadWord(rom, 0x840000 | (header + 2));
            ushort firstDraw = ReadWord(rom, 0x840000 | (initial + 14));
            AssertTrue(ColoredDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"resident colored-door header ${header:X4} selects compiled art");
            plms.Step(guarded, level, streamer, 0, 0, 0);
            bool vertical = (selected.Runs.Span[0].DirectionAndCount & 0x8000) != 0;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).LevelWord,
                    $"resident colored-door ${header:X4} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored-door ${header:X4} avoids program and draw ROM reads");
            if (header == RoomPlmHeaders.GreenDoorFacingLeft)
            {
                AssertEqual((ushort)0x0053, plms.TilemapUpdates[0].TopRow[0],
                    "edited colored cap reaches immediate redraw");
                AssertEqual((ushort)0x0053,
                    level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
                    "edited colored cap survives later camera streaming");
            }

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
            AssertTrue(plms.TrySpawnDoorClosingPlm(guarded, level, enteringDoor, system),
                $"resident colored door ${header:X4} selects its native closing list");
            for (int frame = 0; frame < 20 &&
                 plms.ColoredDoors.Single().Phase == ColoredDoorPhase.Closing; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(ColoredDoorPhase.Waiting, plms.ColoredDoors.Single().Phase,
                $"resident colored door ${header:X4} finishes closing and resumes its owner");

            SamusProjectileTypeWord accepted = header switch
            {
                >= RoomPlmHeaders.YellowDoorFacingLeft and
                    <= RoomPlmHeaders.YellowDoorFacingDown => new(0x0300),
                >= RoomPlmHeaders.GreenDoorFacingLeft and
                    <= RoomPlmHeaders.GreenDoorFacingDown => new(0x0200),
                _ => new(0x0100),
            };
            int hits = header >= RoomPlmHeaders.RedDoorFacingLeft ? 5 : 1;
            for (int hit = 0; hit < hits; hit++)
            {
                AssertTrue(plms.TryNotifyColoredDoorHit(origin, accepted),
                    $"resident colored door ${header:X4} accepts hit {hit + 1}");
                plms.Step(guarded, level, streamer, 0, 0, 0);
            }
            AssertEqual(ColoredDoorPhase.Opening, plms.ColoredDoors.Single().Phase,
                $"resident colored door ${header:X4} reaches opening threshold");
            for (int frame = 0; frame < 128 && plms.ActiveCount != 0; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(0, plms.ActiveCount,
                $"resident colored door ${header:X4} completes opening and deletes");
            AssertTrue(system.HasOpenedDoorBit(0),
                $"resident colored door ${header:X4} persists the opened state");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored door ${header:X4} completes without program/draw ROM reads");

            var reopenedLevel = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], blockDefinitions);
            BackgroundTilemapStreamer reopenedStreamer = reopenedLevel.CreateBackgroundStreamer();
            var reopened = new RoomPlmSystem { ColoredDoorVisuals = stock };
            AssertEqual(1, reopened.LoadRoomPopulation(guarded, reopenedLevel,
                reopenedStreamer, new SnesVram(), population, system,
                AreaId.Crateria, () => new SamusState(), () => false),
                $"opened colored door ${header:X4} reloads");
            reopened.Step(guarded, reopenedLevel, reopenedStreamer, 0, 0, 0);
            AssertEqual(0, reopened.ActiveCount,
                $"opened colored door ${header:X4} converts to a blue cap");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"opened colored door ${header:X4} converts without program/draw ROM reads");
        }
        AssertThrows<InvalidDataException>(
            () => new RoomPlmColoredDoorVisualCatalog(entries.Skip(1)),
            "colored-door catalog rejects missing frames");
        editedFrame.Blocks[0] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmColoredDoorVisualCatalog(entries),
            "colored-door catalog rejects collision bits in visual words");
        editedFrame.Blocks[0] = originalVisual;
        VerifyColoredDoorVisualInstallation(rom);
        Console.WriteLine(
            "  Colored doors: 1164 compiled program bytes, 12 close/hit/open/reload paths, 48 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    private static void VerifyColoredDoorProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        foreach ((ushort first, ushort last) in new[]
        {
            (ColoredDoorPlmProgramDefinitions.YellowStart,
                ColoredDoorPlmProgramDefinitions.YellowEnd),
            (ColoredDoorPlmProgramDefinitions.GreenStart,
                ColoredDoorPlmProgramDefinitions.GreenEnd),
            (ColoredDoorPlmProgramDefinitions.RedStart,
                ColoredDoorPlmProgramDefinitions.RedEnd),
        })
        {
            for (int address = first; address <= last; address++)
            {
                AssertTrue(ColoredDoorPlmProgramDefinitions.TryReadMechanicsByte(
                    checked((ushort)address), out byte compiled),
                    $"colored-door program claims byte $84:{address:X4}");
                AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                    $"colored-door program byte $84:{address:X4} matches ROM");
                if (address == last)
                    continue;
                AssertTrue(ColoredDoorPlmProgramDefinitions.TryReadMechanicsWord(
                    checked((ushort)address), out ushort compiledWord),
                    $"colored-door program claims word $84:{address:X4}");
                ushort native = (ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8);
                AssertEqual(native, compiledWord,
                    $"colored-door program word $84:{address:X4} matches ROM");
            }
        }
        AssertTrue(!ColoredDoorPlmProgramDefinitions.TryReadMechanicsByte(0xbffc, out _),
            "colored-door program does not claim preceding grey-door byte");
        AssertTrue(!ColoredDoorPlmProgramDefinitions.TryReadMechanicsByte(0xc489, out _),
            "colored-door program does not claim following blue-door byte");
        AssertTrue(!ColoredDoorPlmProgramDefinitions.TryReadMechanicsWord(0xc184, out _),
            "yellow list refuses a word crossing into the green list");
    }

    private static void VerifyColoredDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "colored-door-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Colored-door test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmColoredDoorVisualFiles.Extract(rom,
                installation.RoomPlmColoredDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmColoredDoorVisualFiles.ValidateStock(
                installation.RoomPlmColoredDoorVisualDirectory);
            ushort stock = installation.LoadRoomPlmColoredDoorVisuals().GetWord(0xa827, 0);
            string stockPath = Path.Combine(installation.RoomPlmColoredDoorVisualDirectory,
                RoomPlmColoredDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted colored-door JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "green-left-frame-0")!;
            frame["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmColoredDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmColoredDoorVisualOverrideDirectory,
                RoomPlmColoredDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmColoredDoorVisuals().GetWord(0xa827, 0),
                "installed colored-door override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmColoredDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmColoredDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmColoredDoorVisualOverrideDirectory)
                    .GetWord(0xa827, 0),
                "colored-door override survives stock replacement");

            frame["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmColoredDoorVisuals(),
                "invalid colored-door override fails loudly");
            frame["blocks"]![0] = stock;
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmColoredDoorVisuals(),
                "tampered colored-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class ColoredDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= 0x84bffd && address <= 0x84c488)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Resident colored door reread compiled instruction ${address:X6}.");
            }
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                if (address >= first &&
                    address < first + ColoredDoorPlmDrawDefinitions.DrawListBytes)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Resident colored door reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
