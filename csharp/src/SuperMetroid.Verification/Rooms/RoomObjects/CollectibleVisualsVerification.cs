using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupCollectibleDrawsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupCollectibleSelectorsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupCollectibleStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
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
        InstalledRoomObjectCollectibleVisuals stock = InstalledRoomObjectCollectibleVisuals.Stock();

        Suite(nameof(VerifyCollectibleOrbSelector), () => VerifyCollectibleOrbSelector(rom));
        Suite(nameof(VerifyCollectibleRevealSelector), () => VerifyCollectibleRevealSelector(rom));
        Suite(nameof(VerifyCollectibleTankSelector), () => VerifyCollectibleTankSelector(rom));
        Suite(nameof(VerifyCollectibleDynamicSelector), () => VerifyCollectibleDynamicSelector(rom));

        CollectibleVisualsRenderResult Render(
            InstalledRoomObjectCollectibleVisuals visuals)
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, 0xeed7, roomArgument: 0, precollected: false);
            RoomObjectFixtures.VisualsOf(fixture.RoomObjects).RoomObjectCollectibleVisuals = visuals;
            fixture.RoomObjects.Step(fixture.Level);
            var result = new CollectibleVisualsRenderResult(fixture.Level.GetCollisionBlockByIndex(fixture.BlockIndex).Terrain,
                fixture.Level.DisplayedAppearance(fixture.BlockIndex),
                fixture.Level.DisplayedAppearance(fixture.BlockIndex));
            AssertTrue(fixture.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(fixture.Block),
                "the item keeps its native touch trigger active");
            fixture.RoomObjects.Step(fixture.Level);
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
            // Each kind stands at its own retail item location, numbered from the first of the contiguous run 128 through 152.
            CollectibleFixture fixture = LoadCollectible(bus, checked((ushort)(0xeed7 + kind * 4)), checked((ushort)(128 + kind)), precollected: false);
            RoomObjectFixtures.VisualsOf(fixture.RoomObjects).RoomObjectCollectibleVisuals = stock;
            fixture.RoomObjects.Step(fixture.Level);
            AssertEqual(1, fixture.RoomObjects.CollectibleObjects.Collectibles.Count,
                $"item kind {kind} still owns its live pickup after ROM-free drawing");
        }
        foreach (ushort header in new ushort[] { 0xef2b, 0xef7f })
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            CollectibleFixture fixture = LoadCollectible(bus, header, 0, false);
            RoomObjectFixtures.VisualsOf(fixture.RoomObjects).RoomObjectCollectibleVisuals = stock;
            if (header == 0xef7f)
                AssertTrue(fixture.RoomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(
                        fixture.Block),
                    "shot-block collectible accepts its native trigger");
            fixture.RoomObjects.Step(fixture.Level);
            AssertEqual(1, fixture.RoomObjects.CollectibleObjects.Collectibles.Count,
                $"collectible presentation ${header:X4} survives ROM-free drawing");
        }

        Suite(nameof(VerifyCollectibleVisualInstallation), () => VerifyCollectibleVisualInstallation(rom));
        Console.WriteLine(
            "Collectible visuals: 24 native draw lists, eight block-pair selectors, " +
            "all 21 item kinds, orb/reveal paths, " +
            "source-read guards, and installation validation pass.");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyCollectibleVisualInstallation(CartridgeImportAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectCollectibleVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectCollectibleVisualFilesConstants.ValidateStock(
            installation.RoomObjectCollectibleVisualDirectory);
        VerifyCollectibleStockMapping(rom, installation.LoadRoomObjectCollectibleVisuals());

        string stockPath = Path.Combine(installation.RoomObjectCollectibleVisualDirectory,
            RoomObjectCollectibleVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectCollectibleVisuals(),
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
