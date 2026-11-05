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
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks collectible PLM art and live pickup behavior.</summary>
internal static class CollectibleVisualsVerification
{
    /// <summary>Compares 24 native draw lists, exercises all item kinds, and checks the stock tile's redraw.</summary>
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
                "the item keeps its native touch trigger active");
            fixture.Plms.Step(fixture.Level);
            AssertEqual(199, fixture.Samus.MaxHealth,
                "the energy tank grants its native pickup reward");
            AssertTrue(fixture.System.HasCollectedItemBit(0),
                "the energy tank sets its native persistence bit");
            return result;
        }

        var original = Render(stock);
        AssertEqual(RoomTerrainBlock.Of(0x04a, BlockMirroring.None, RoomCollisionType.SpecialBlock), original.Physical,
            "stock energy tank retains the cartridge physical level word");
        // The block BG1 shows names its picture (#1431): the stock frame's block.
        AssertEqual(RoomBlockAppearance.Of(0x04a), original.Immediate,
            "stock collectible block reaches the immediate redraw");

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
            "all 21 item kinds, orb/reveal paths, " +
            "source-read guards, and installation validation pass.");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyCollectibleVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmCollectibleVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomPlmCollectibleVisualFilesConstants.ValidateStock(
            installation.RoomPlmCollectibleVisualDirectory);
        VerifyCollectibleStockMapping(rom, installation.LoadRoomPlmCollectibleVisuals());

        string stockPath = Path.Combine(installation.RoomPlmCollectibleVisualDirectory,
            RoomPlmCollectibleVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomPlmCollectibleVisuals(),
            "collectible stock manifest rejects corruption");
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
