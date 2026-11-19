using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Confirms a collectible shows its kind's own picture where it lies, whatever was set up before it (#1538), while the
/// terrain its frames write keeps the block pair the room's collectibles take in turn (<see cref="CollectibleBlockPairs"/>),
/// which a Wave shot's straddled read sees.
/// </summary>
internal static class CollectiblePicturesVerification
{
    /// <summary>
    /// The room's five items that show their own picture, in population order, each held in a Chozo statue's orb as rooms
    /// place them (#2354): the fifth takes the first item's block pair again.
    /// </summary>
    private static readonly (RoomObjectKind Header, InWorldCollectibleKind Kind)[] Items =
    [
        (RoomObjectKind.ChozoBombs, InWorldCollectibleKind.Bombs),
        (RoomObjectKind.ChozoChargeBeam, InWorldCollectibleKind.ChargeBeam),
        (RoomObjectKind.ChozoIceBeam, InWorldCollectibleKind.IceBeam),
        (RoomObjectKind.ChozoHiJumpBoots, InWorldCollectibleKind.HiJumpBoots),
        (RoomObjectKind.ChozoSpeedBooster, InWorldCollectibleKind.SpeedBooster),
    ];

    /// <summary>
    /// Loads a synthetic room placing five Chozo-orb items, shoots their orbs open and draws their visible frames: each
    /// item's terrain is its block pair's block under the look it shows (#2354), the fifth sharing the first's pair, and
    /// each shows its own kind's picture in that look, the fifth too.
    /// </summary>
    internal static void VerifyCollectiblePictures()
    {
        var pairs = new CollectibleBlockPairs();
        CollectibleBlockPairs.Pair[] taken = [.. Enumerable.Range(0, Items.Length).Select(_ => pairs.Take())];
        AssertTrue(taken.SequenceEqual([CollectibleBlockPairs.Pair.First, CollectibleBlockPairs.Pair.Second,
                CollectibleBlockPairs.Pair.Third, CollectibleBlockPairs.Pair.Fourth, CollectibleBlockPairs.Pair.First]),
            "collectibles take the four block pairs in turn, the fifth taking the first again");

        var bus = new TestAddressSpace();
        CollectiblesVerification.SeedCollectibleRom(bus);
        const ushort population = 0x9000;
        var size = new Extent16<RoomBlocks>(8, 8);
        var records = new List<byte>();
        for (int item = 0; item < Items.Length; item++)
        {
            ushort header = RoomObjectNativeWords.Word(Items[item].Header);
            records.AddRange([unchecked((byte)header), LittleEndianBytes.HighByte(header), unchecked((byte)(item + 1)), 1, unchecked((byte)item), 0]);
        }
        records.AddRange([0x00, 0x00]);
        SeedRoomObjectPopulation(bus, population, records.ToArray());
        var foreground = new RoomTerrainBlock[size.Area];
        RoomLevelData level = CreateRoom(size, foreground, new RoomBlockBehavior[foreground.Length]);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 99, MaxHealth = 99 };
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria),
            RoomObjectPopulationImporter.Read(bus, population),
            RoomObjectServiceFixtures.Uninstalled with { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        roomObjects.Step(level);
        for (int item = 0; item < Items.Length; item++)
        {
            roomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(new Position16<RoomBlocks>((ushort)(item + 1), 1));
        }
        StepUntil(
            () => roomObjects.CollectiblePhases.All(phase => phase == CollectiblePhase.Visible),
            _ => roomObjects.Step(level),
            maximumFrames: 20,
            context: "the orbs' bursts");
        for (int item = 0; item < Items.Length; item++)
        {
            int blockIndex = size.RowMajorIndex(new Position16<RoomBlocks>((ushort)(item + 1), 1));
            RoomBlockAppearance written = level.GetCollisionBlockByIndex(blockIndex).Terrain.Appearance;
            AssertTrue(written.IsCollectiblePairBlock && written.PairBlock.Pair == taken[item],
                $"{Items[item].Kind} writes a block of the block pair it took into the terrain, not {written}");
            AssertEqual(RoomCellPicture.Of(new CollectiblePicture(Items[item].Kind, written.PairBlock.Look)),
                level.ForegroundPlane.Cells.Cells[blockIndex].Picture,
                $"{Items[item].Kind} shows its own picture in the look its pair block lies under");
        }
        Console.WriteLine($"Collectible pictures: {Items.Length} items each show their own picture over the block pairs they take in turn.");
    }

    /// <summary>Runs the collectible picture check.</summary>
    [VerificationCommand("--collectible-pictures")]
    internal static int CollectiblePicturesCommand()
    {
        Suite(nameof(VerifyCollectiblePictures), VerifyCollectiblePictures);
        return 0;
    }
}
