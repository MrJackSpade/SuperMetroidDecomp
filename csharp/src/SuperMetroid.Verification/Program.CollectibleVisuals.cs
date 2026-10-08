using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCollectibleVisuals()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Collectible oracle revision");
        Suite(nameof(VerifyCollectibleDrawGeometry), () => VerifyCollectibleDrawGeometry(rom));
        Suite(nameof(VerifyCollectibleDrawCollision), () => VerifyCollectibleDrawCollision(rom));
        Suite(nameof(VerifyCollectibleDrawVisuals), () => VerifyCollectibleDrawVisuals(rom));
        Suite(nameof(VerifyCollectibleDrawIdentity), () => VerifyCollectibleDrawIdentity(rom));
        RoomPlmCollectibleVisualCatalog stock = RoomPlmCollectibleVisualCatalog.Stock();

        Suite(nameof(VerifyCollectibleOrbSelector), () => VerifyCollectibleOrbSelector(rom));
        Suite(nameof(VerifyCollectibleRevealSelector), () => VerifyCollectibleRevealSelector(rom));
        Suite(nameof(VerifyCollectibleTankSelector), () => VerifyCollectibleTankSelector(rom));
        Suite(nameof(VerifyCollectibleDynamicSelector), () => VerifyCollectibleDynamicSelector(rom));

        RoomPlmCollectibleVisualEntry[] entries =
            RoomPlmCollectibleDrawDefinitions.All.ToArray()
                .Select(frame => new RoomPlmCollectibleVisualEntry(frame.Id,
                    new RoomLevelWord(frame.LevelWord).VisualWord))
                .ToArray();
        int energyIndex = Array.FindIndex(entries, entry => entry.Id == "energy-tank-0");
        entries[energyIndex] = entries[energyIndex] with { VisualWord = 0x0053 };
        var edited = new RoomPlmCollectibleVisualCatalog(entries);
        entries[energyIndex] = entries[energyIndex] with { VisualWord = 0x0054 };
        AssertEqual((ushort)0x0053,
            edited.GetWord(RoomPlmCollectibleDrawDefinitions.TankFirst),
            "collectible catalog copies the edited visual word");

        (ushort physical, ushort immediate, ushort streamed) Render(
            RoomPlmCollectibleVisualCatalog visuals)
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(
                bus, 0xeed7, roomArgument: 0, precollected: false);
            fixture.Plms.CollectibleVisuals = visuals;
            fixture.Level.SetBlockDefinitionWord(0x04a * 4, 0x0123);
            fixture.Level.SetBlockDefinitionWord(0x053 * 4, 0x0456);
            fixture.Streamer.SetBlockDefinitionWord(0x04a * 4, 0x0123);
            fixture.Streamer.SetBlockDefinitionWord(0x053 * 4, 0x0456);
            fixture.Plms.Step(new CollectibleDrawReadGuard(bus),
                fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertTrue(fixture.Plms.TilemapUpdates.Count > 0,
                "collectible frame publishes an immediate BG1 redraw");
            var result = (fixture.Level.GetCollisionBlockByIndex(fixture.BlockIndex).LevelWord,
                fixture.Plms.TilemapUpdates[0].TopRow[0],
                fixture.Streamer.BuildPlmLevelBlockUpdate(fixture.BlockIndex, 0).TopRow[0]);
            AssertTrue(fixture.Plms.TryNotifyCollectibleTouch(fixture.BlockIndex),
                "edited item art leaves the native touch trigger active");
            fixture.Plms.Step(new CollectibleDrawReadGuard(bus),
                fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual(199, fixture.Samus.MaxHealth,
                "edited energy-tank art leaves the pickup reward unchanged");
            AssertTrue(fixture.System.HasCollectedItemBit(0),
                "edited energy-tank art leaves persistence unchanged");
            return result;
        }

        var original = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0xb04a, original.physical,
            "stock energy tank retains the cartridge physical level word");
        AssertEqual(original.physical, changed.physical,
            "collectible art does not change physical collision or pickup state");
        AssertEqual((ushort)0x0123, original.immediate,
            "stock collectible tile reaches the immediate redraw");
        AssertEqual((ushort)0x0456, changed.immediate,
            "edited collectible tile reaches the immediate redraw");
        AssertEqual((ushort)0x0456, changed.streamed,
            "edited collectible tile survives later camera streaming");

        for (int kind = 0; kind < 21; kind++)
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus,
                checked((ushort)(0xeed7 + kind * 4)),
                checked((ushort)kind), precollected: false);
            fixture.Plms.CollectibleVisuals = stock;
            fixture.Plms.Step(new CollectibleDrawReadGuard(bus),
                fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual(1, fixture.Plms.Collectibles.Count,
                $"item kind {kind} still owns its live pickup after ROM-free drawing");
        }
        foreach (ushort header in new ushort[] { 0xef2b, 0xef7f })
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, header, 0, false);
            fixture.Plms.CollectibleVisuals = stock;
            if (header == 0xef7f)
                AssertTrue(fixture.Plms.TryNotifyCollectibleProjectileHit(
                        fixture.BlockIndex, 0x0500),
                    "shot-block collectible accepts its native trigger");
            fixture.Plms.Step(new CollectibleDrawReadGuard(bus),
                fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual(1, fixture.Plms.Collectibles.Count,
                $"collectible presentation ${header:X4} survives ROM-free drawing");
        }

        Suite(nameof(VerifyCollectibleVisualInstallation), () => VerifyCollectibleVisualInstallation(rom));
        Console.WriteLine(
            "Collectible visuals: 24 native draw lists, eight dynamic selectors, " +
            "all 21 item kinds, orb/reveal paths, live edits, physical isolation, " +
            "source-read guards, and installation validation pass.");
    }

    private static void VerifyCollectibleVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "collectible-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Collectible visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmCollectibleVisualFiles.Extract(rom,
                installation.RoomPlmCollectibleVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmCollectibleVisualFiles.ValidateStock(
                installation.RoomPlmCollectibleVisualDirectory);
            ushort pointer = RoomPlmCollectibleDrawDefinitions.TankFirst;
            VerifyCollectibleStockMapping(rom, installation.LoadRoomPlmCollectibleVisuals());

            string stockPath = Path.Combine(installation.RoomPlmCollectibleVisualDirectory,
                RoomPlmCollectibleVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted collectible JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "energy-tank-0")!;
            frame["visualWord"] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmCollectibleVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmCollectibleVisualOverrideDirectory,
                RoomPlmCollectibleVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmCollectibleVisuals().GetWord(pointer),
                "collectible visual override selects edited tank frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmCollectibleVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmCollectibleVisualFiles.Load(refreshed,
                    installation.RoomPlmCollectibleVisualOverrideDirectory).GetWord(pointer),
                "collectible override survives stock replacement");

            frame["visualWord"] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmCollectibleVisuals(),
                "collectible override rejects collision-bit edits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmCollectibleVisuals(),
                "collectible stock manifest rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private static ushort ReadCollectibleVisualWord(ISnesAddressSpace bus,
        ushort pointer) => unchecked((ushort)(
        bus.ReadByte(0x840000 | pointer) |
        bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    private sealed class CollectibleDrawReadGuard : ISnesAddressSpace, IImportCartridgeSource
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = [];

        internal CollectibleDrawReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            foreach (RoomPlmCollectibleDrawFrame frame in RoomPlmCollectibleDrawDefinitions.All)
                for (int offset = 0; offset < 6; offset++)
                    blocked.Add(0x840000 | checked((ushort)(frame.Pointer + offset)));
            foreach (ushort table in new ushort[]
                     { RoomPlmCollectibleDrawDefinitions.DynamicFrame0Table,
                       RoomPlmCollectibleDrawDefinitions.DynamicFrame1Table })
                for (int offset = 0; offset < 8; offset++)
                    blocked.Add(0x840000 | checked((ushort)(table + offset)));
        }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidOperationException(
                $"Live collectible read compiled draw or selector byte ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
