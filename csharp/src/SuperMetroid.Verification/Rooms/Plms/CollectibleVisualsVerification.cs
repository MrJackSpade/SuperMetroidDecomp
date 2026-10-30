using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
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
        InstalledRoomPlmCollectibleVisuals stock = InstalledRoomPlmCollectibleVisuals.Stock();

        Suite(nameof(VerifyCollectibleOrbSelector), () => VerifyCollectibleOrbSelector(rom));
        Suite(nameof(VerifyCollectibleRevealSelector), () => VerifyCollectibleRevealSelector(rom));
        Suite(nameof(VerifyCollectibleTankSelector), () => VerifyCollectibleTankSelector(rom));
        Suite(nameof(VerifyCollectibleDynamicSelector), () => VerifyCollectibleDynamicSelector(rom));

        RoomPlmVisualBlockEntry[] entries =
            CollectiblePlmDraws.All
                .Select(frame => new RoomPlmVisualBlockEntry(frame.Id,
                    frame.Terrain.Appearance))
                .ToArray();
        int energyIndex = Array.FindIndex(entries, entry => entry.Id == "energy-tank-0");
        entries[energyIndex] = entries[energyIndex] with { Appearance = RoomBlockAppearance.Of(0x053) };
        var edited = new InstalledRoomPlmCollectibleVisuals(entries);
        entries[energyIndex] = entries[energyIndex] with { Appearance = RoomBlockAppearance.Of(0x054) };
        AssertEqual(RoomBlockAppearance.Of(0x053),
            edited.GetAppearance(CollectiblePlmDraws.VisibleFrame(InWorldCollectibleKind.EnergyTank, 0, 0)),
            "collectible catalog copies the edited visual word");

        CollectibleVisualsRenderResult Render(
            InstalledRoomPlmCollectibleVisuals visuals)
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, 0xeed7, roomArgument: 0, precollected: false);
            PlmFixtures.VisualsOf(fixture.Plms).RoomPlmCollectibleVisuals = visuals;
            fixture.Plms.Step(fixture.Level);
            var result = new CollectibleVisualsRenderResult(fixture.Level.GetCollisionBlockByIndex(fixture.BlockIndex).Terrain,
                fixture.Level.DisplayedAppearance(fixture.BlockIndex),
                fixture.Level.DisplayedAppearance(fixture.BlockIndex));
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
        AssertEqual(RoomTerrainBlock.Of(0x04a, BlockMirroring.None, RoomCollisionType.SpecialBlock), original.Physical,
            "stock energy tank retains the cartridge physical level word");
        AssertEqual(original.Physical, changed.Physical,
            "collectible art does not change physical collision or pickup state");
        // The block BG1 shows names its picture (#1431): the stock frame's block, or the edited frame's.
        AssertEqual(RoomBlockAppearance.Of(0x04a), original.Immediate,
            "stock collectible block reaches the immediate redraw");
        AssertEqual(RoomBlockAppearance.Of(0x053), changed.Immediate,
            "edited collectible block reaches the immediate redraw");
        AssertEqual(RoomBlockAppearance.Of(0x053), changed.Streamed,
            "edited collectible block survives later camera streaming");

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
            frame["appearance"] = PlmVisualJson.Appearance(0x053);
            Directory.CreateDirectory(installation.RoomPlmCollectibleVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmCollectibleVisualOverrideDirectory,
                RoomPlmCollectibleVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x053),
                installation.LoadRoomPlmCollectibleVisuals().GetAppearance(NativePlmDraws.At<RoomPlmCollectibleDrawDefinitions.Draw>(pointer)),
                "collectible visual override selects edited tank frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmCollectibleVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x053),
                RoomPlmCollectibleVisualFiles.Load(refreshed,
                    installation.RoomPlmCollectibleVisualOverrideDirectory).GetAppearance(NativePlmDraws.At<RoomPlmCollectibleDrawDefinitions.Draw>(pointer)),
                "collectible override survives stock replacement");

            frame["appearance"] = 0xf053;
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
    internal static ushort ReadCollectibleDrawWord(ISnesAddressSpace bus,
        ushort pointer) => CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(0x840000 | pointer);

    /// <summary>Runs collectible visual and pickup isolation checks.</summary>
    [VerificationCommand("--collectible-visuals")]
    internal static int CollectibleVisualsCommand()
    {
        Suite(nameof(VerifyCollectibleVisuals), () => VerifyCollectibleVisuals());
        return 0;
    }
}
