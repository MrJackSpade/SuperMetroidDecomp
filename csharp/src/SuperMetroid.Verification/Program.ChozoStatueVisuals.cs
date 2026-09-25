using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyChozoStatueVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "chozo-statue-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Chozo statue test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction checks every physical word, run direction and offset
            // against the pinned cartridge before writing editable art.
            RoomPlmChozoStatueVisualFiles.Extract(rom,
                installation.RoomPlmChozoStatueVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmChozoStatueVisualFiles.ValidateStock(
                installation.RoomPlmChozoStatueVisualDirectory);
            AssertEqual((ushort)0x012b,
                installation.LoadRoomPlmChozoStatueVisuals().GetWord(
                    ChozoStatuePlmDrawDefinitions.ClearSlopeAccess, 0, 0),
                "stock clear-slope visual matches cartridge");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmChozoStatueVisualCatalog(
                    [new RoomPlmChozoStatueVisualEntry(
                        "wrecked-ship-clear-slope-access", new ushort[1])]),
                "Chozo catalog rejects incomplete layout set");

            string stockPath = Path.Combine(
                installation.RoomPlmChozoStatueVisualDirectory,
                RoomPlmChozoStatueVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Chozo JSON is empty.");
            JsonNode clearSlope = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() ==
                "wrecked-ship-clear-slope-access")!;
            clearSlope["blocks"]![0] = 0x0053;
            JsonNode blockSlope = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() ==
                "wrecked-ship-block-slope-access")!;
            blockSlope["blocks"]![0] = 0x0054;
            Directory.CreateDirectory(
                installation.RoomPlmChozoStatueVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmChozoStatueVisualOverrideDirectory,
                RoomPlmChozoStatueVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            var selected = installation.LoadRoomPlmChozoStatueVisuals();
            AssertEqual((ushort)0x0053, selected.GetWord(
                ChozoStatuePlmDrawDefinitions.ClearSlopeAccess, 0, 0),
                "Chozo art override selects its edited first block");

            // Run both real Chozo PLM instruction lists. The bus refuses reads
            // of compiled draw bytes, so the production dispatcher must reach
            // each extracted override without falling back to ROM data.
            const int width = 32;
            var blockDefinitions = new byte[0x400 * 8];
            blockDefinitions[0x53 * 8] = 0x53;
            blockDefinitions[0x54 * 8] = 0x54;
            var guarded = new ChozoProgramAndDrawReadGuard(rom);
            foreach ((ushort header, ushort physical, ushort visual) in new[]
            {
                (ChozoStatuePlmRomData.ClearSlopeAccess, (ushort)0x012b, (ushort)0x0053),
                (ChozoStatuePlmRomData.BlockSlopeAccess, (ushort)0xa12b, (ushort)0x0054),
            })
            {
                var level = CreateRoom(width, 16,
                    new ushort[width * 16], new byte[width * 16],
                    blockDefinitions: blockDefinitions);
                BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
                var plms = new RoomPlmSystem { ChozoStatueVisuals = selected };
                AssertTrue(plms.TrySpawnChozoStatuePlm(level,
                    new ChozoStatuePlmRequest(header, 3, 4, IsHardcoded: false)),
                    $"Chozo actor ${header:X4} spawns in a bounded test room");
                plms.Step(guarded, level, streamer, 0, 0, 0);
                int origin = 4 * width + 3;
                AssertEqual(physical,
                    level.GetCollisionBlockByIndex(origin).LevelWord,
                    $"edited Chozo art retains native physical word for ${header:X4}");
                AssertEqual(visual,
                    level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                        origin, 0).TopRow[0],
                    $"edited Chozo art reaches streamed tilemap for ${header:X4}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Chozo slope PLMs did not reread compiled program or draw bytes");

            var crumbleLevel = CreateRoom(width, 16,
                new ushort[width * 16], new byte[width * 16],
                blockDefinitions: blockDefinitions);
            var crumblePlms = new RoomPlmSystem();
            AssertTrue(crumblePlms.TrySpawnChozoStatuePlm(crumbleLevel,
                new ChozoStatuePlmRequest(ChozoStatuePlmRomData.CrumblePlug,
                    3, 4, IsHardcoded: false)),
                "Chozo crumble plug spawns in a bounded test room");
            var seenCrumbleWords = new HashSet<ushort>();
            for (int frame = 0; frame < 15; frame++)
            {
                crumblePlms.Step(guarded, crumbleLevel,
                    crumbleLevel.CreateBackgroundStreamer(), 0, 0, 0);
                seenCrumbleWords.Add(crumbleLevel.GetCollisionBlock(3, 4).LevelWord);
            }
            foreach (ushort word in new ushort[] { 0x0053, 0x0054, 0x0055, 0x00ff })
                AssertTrue(seenCrumbleWords.Contains(word),
                    $"Chozo crumble program draws frame ${word:X4}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Chozo crumble PLM did not reread compiled program or draw bytes");

            var wreckedHandLevel = CreateRoom(width, 16,
                new ushort[width * 16], new byte[width * 16],
                blockDefinitions: blockDefinitions);
            var wreckedHandPlms = new RoomPlmSystem();
            AssertTrue(wreckedHandPlms.TrySpawnChozoStatuePlm(wreckedHandLevel,
                new ChozoStatuePlmRequest(ChozoStatuePlmRomData.WreckedShipHand,
                    3, 4, IsHardcoded: false)),
                "Wrecked Ship hand spawns from its compiled header");
            wreckedHandPlms.Step(guarded, wreckedHandLevel,
                wreckedHandLevel.CreateBackgroundStreamer(), 0, 0, 0);
            AssertTrue(wreckedHandPlms.PopulationSlots.Count == 0,
                "Wrecked Ship hand executes the shared compiled delete list");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "shared delete list did not reread its bank-$84 bytes");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmChozoStatueVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmChozoStatueVisualFiles.Load(refreshed,
                    installation.RoomPlmChozoStatueVisualOverrideDirectory)
                    .GetWord(ChozoStatuePlmDrawDefinitions.ClearSlopeAccess, 0, 0),
                "Chozo override survives stock replacement");

            clearSlope["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmChozoStatueVisuals(),
                "Chozo override rejects physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmChozoStatueVisualFiles.ValidateStock(
                    installation.RoomPlmChozoStatueVisualDirectory),
                "tampered Chozo stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class ChozoProgramAndDrawReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if ((address >= (0x840000 | ChozoStatuePlmProgramDefinitions.CrumblePlugStart) &&
                 address <= (0x840000 | ChozoStatuePlmProgramDefinitions.CrumblePlugEnd)) ||
                (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.LowerNorfairHandStart) &&
                 address <= (0x840000 | ChozoStatuePlmProgramDefinitions.LowerNorfairHandEnd)) ||
                (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.ClearSlopeStart) &&
                 address <= (0x840000 | ChozoStatuePlmProgramDefinitions.ClearSlopeEnd)) ||
                (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.BlockSlopeStart) &&
                 address <= (0x840000 | ChozoStatuePlmProgramDefinitions.BlockSlopeEnd)) ||
                (address >= (0x840000 | RoomPlmSharedDeleteProgramDefinitions.Start) &&
                 address <= (0x840000 | RoomPlmSharedDeleteProgramDefinitions.End)) ||
                ChozoStatuePlmDrawDefinitions.All.Any(draw =>
            {
                int first = 0x840000 | draw.Pointer;
                int length = draw.Runs.Span.ToArray().Sum(run =>
                    4 + run.LevelWords.Length * 2);
                return address >= first && address < first + length;
            }))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Chozo PLM reread compiled program or draw byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
