using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupCollectibleDrawsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupCollectibleSelectorsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupCollectibleStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks collectible PLM art, live pickup behavior, and visual-only overrides.</summary>
internal static class CollectibleVisualsVerification
{
    /// <summary>Compares 24 native draw lists, exercises all item kinds, and checks edited tile streaming.</summary>
    internal static void VerifyCollectibleVisuals()
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

        RoomPlmVisualWordEntry[] entries =
            RoomPlmCollectibleDrawDefinitions.All
                .Select(frame => new RoomPlmVisualWordEntry(frame.Id,
                    frame.LevelWord.VisualWord))
                .ToArray();
        int energyIndex = Array.FindIndex(entries, entry => entry.Id == "energy-tank-0");
        entries[energyIndex] = entries[energyIndex] with { VisualWord = 0x0053 };
        var edited = new RoomPlmCollectibleVisualCatalog(entries);
        entries[energyIndex] = entries[energyIndex] with { VisualWord = 0x0054 };
        AssertEqual((ushort)0x0053,
            edited.GetWord(RoomPlmCollectibleDrawDefinitions.VisibleFrame(InWorldCollectibleKind.EnergyTank, 0, 0)),
            "collectible catalog copies the edited visual word");

        CollectibleVisualsRenderResult Render(
            RoomPlmCollectibleVisualCatalog visuals)
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, 0xeed7, roomArgument: 0, precollected: false);
            PlmFixtures.VisualsOf(fixture.Plms).RoomPlmCollectibleVisuals = visuals;
            fixture.Level.SetBlockDefinitionWord(0x04a * 4, 0x0123);
            fixture.Level.SetBlockDefinitionWord(0x053 * 4, 0x0456);
            fixture.Plms.Step(fixture.Level);
            var result = new CollectibleVisualsRenderResult(fixture.Level.GetCollisionBlockByIndex(fixture.BlockIndex).PackedWord.Raw,
                fixture.Level.DisplayedBlockTiles(fixture.BlockIndex).TopLeft.Raw,
                fixture.Level.DisplayedBlockTiles(fixture.BlockIndex).TopLeft.Raw);
            AssertTrue(fixture.Plms.CollectiblePlms.TryNotifyCollectibleTouch(fixture.BlockIndex),
                "edited item art leaves the native touch trigger active");
            fixture.Plms.Step(fixture.Level);
            AssertEqual(199, fixture.Samus.MaxHealth,
                "edited energy-tank art leaves the pickup reward unchanged");
            AssertTrue(fixture.System.HasCollectedItemBit(0),
                "edited energy-tank art leaves persistence unchanged");
            return result;
        }

        var original = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0xb04a, original.Physical,
            "stock energy tank retains the cartridge physical level word");
        AssertEqual(original.Physical, changed.Physical,
            "collectible art does not change physical collision or pickup state");
        AssertEqual((ushort)0x0123, original.Immediate,
            "stock collectible tile reaches the immediate redraw");
        AssertEqual((ushort)0x0456, changed.Immediate,
            "edited collectible tile reaches the immediate redraw");
        AssertEqual((ushort)0x0456, changed.Streamed,
            "edited collectible tile survives later camera streaming");

        for (int kind = 0; kind < 21; kind++)
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, checked((ushort)(0xeed7 + kind * 4)), checked((ushort)kind), precollected: false);
            PlmFixtures.VisualsOf(fixture.Plms).RoomPlmCollectibleVisuals = stock;
            fixture.Plms.Step(fixture.Level);
            AssertEqual(1, fixture.Plms.CollectiblePlms.Collectibles.Count,
                $"item kind {kind} still owns its live pickup after ROM-free drawing");
        }
        foreach (ushort header in new ushort[] { 0xef2b, 0xef7f })
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, header, 0, false);
            PlmFixtures.VisualsOf(fixture.Plms).RoomPlmCollectibleVisuals = stock;
            if (header == 0xef7f)
                AssertTrue(fixture.Plms.CollectiblePlms.TryNotifyCollectibleProjectileHit(
                        fixture.BlockIndex),
                    "shot-block collectible accepts its native trigger");
            fixture.Plms.Step(fixture.Level);
            AssertEqual(1, fixture.Plms.CollectiblePlms.Collectibles.Count,
                $"collectible presentation ${header:X4} survives ROM-free drawing");
        }

        Suite(nameof(VerifyCollectibleVisualInstallation), () => VerifyCollectibleVisualInstallation(rom));
        Console.WriteLine(
            "Collectible visuals: 24 native draw lists, eight dynamic selectors, " +
            "all 21 item kinds, orb/reveal paths, live edits, physical isolation, " +
            "source-read guards, and installation validation pass.");
    }

    /// <summary>Checks extracted stock, selected tank-frame overrides, replacement stock, and invalid visual bits.</summary>
    private static void VerifyCollectibleVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("collectible-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmCollectibleVisualFiles.Extract(rom,
                installation.RoomPlmCollectibleVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmCollectibleVisualFiles.ValidateStock(
                installation.RoomPlmCollectibleVisualDirectory);
            ushort pointer = PlmDrawSourceDefinitions.CollectibleTankFirst.Offset;
            VerifyCollectibleStockMapping(rom, installation.LoadRoomPlmCollectibleVisuals());

            string stockPath = Path.Combine(installation.RoomPlmCollectibleVisualDirectory,
                RoomPlmCollectibleVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted collectible JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "energy-tank-0");
            frame["visualWord"] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmCollectibleVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmCollectibleVisualOverrideDirectory,
                RoomPlmCollectibleVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmCollectibleVisuals().GetWord(NativePlmDraws.At<RoomPlmCollectibleDrawDefinitions.Draw>(pointer)),
                "collectible visual override selects edited tank frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmCollectibleVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmCollectibleVisualFiles.Load(refreshed,
                    installation.RoomPlmCollectibleVisualOverrideDirectory).GetWord(NativePlmDraws.At<RoomPlmCollectibleDrawDefinitions.Draw>(pointer)),
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

    /// <summary>Reads one collectible draw word from bank 84 for the native comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for collectible visual words")]
    internal static ushort ReadCollectibleVisualWord(ISnesAddressSpace bus,
        ushort pointer) => CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(0x840000 | pointer);

    /// <summary>Runs collectible visual and pickup isolation checks.</summary>
    [VerificationCommand("--collectible-visuals")]
    internal static int CollectibleVisualsCommand()
    {
        Suite(nameof(VerifyCollectibleVisuals), () => VerifyCollectibleVisuals());
        return 0;
    }
}
