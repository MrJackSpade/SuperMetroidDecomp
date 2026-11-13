using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
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
    /// The room's five items that show their own picture, in population order: the fifth takes the first item's block
    /// pair again.
    /// </summary>
    private static readonly InWorldCollectibleKind[] Kinds =
    [
        InWorldCollectibleKind.Bombs, InWorldCollectibleKind.ChargeBeam, InWorldCollectibleKind.IceBeam,
        InWorldCollectibleKind.HiJumpBoots, InWorldCollectibleKind.SpeedBooster,
    ];

    /// <summary>The block written under the first block pair's first frame.</summary>
    private const int FirstPairFirstFrameBlock = 0x08e;

    /// <summary>
    /// Loads a synthetic room placing five exposed items and draws their first frames: each item's terrain is its block
    /// pair's first-frame block, the fifth sharing the first's, and each shows its own kind's picture, the fifth too.
    /// </summary>
    internal static void VerifyCollectiblePictures()
    {
        var pairs = new CollectibleBlockPairs();
        CollectibleBlockPairs.Pair[] taken = [.. Enumerable.Range(0, Kinds.Length).Select(_ => pairs.Take())];
        AssertTrue(taken.SequenceEqual([CollectibleBlockPairs.Pair.First, CollectibleBlockPairs.Pair.Second,
                CollectibleBlockPairs.Pair.Third, CollectibleBlockPairs.Pair.Fourth, CollectibleBlockPairs.Pair.First]),
            "collectibles take the four block pairs in turn, the fifth taking the first again");

        var bus = new TestAddressSpace();
        CollectiblesVerification.SeedCollectibleRom(bus);
        const ushort population = 0x9000;
        var size = new Extent16<RoomBlocks>(8, 8);
        var records = new List<byte>();
        for (int item = 0; item < Kinds.Length; item++)
        {
            ushort header = unchecked((ushort)(0xeed7 + (int)Kinds[item] * 4));
            records.AddRange([unchecked((byte)header), LittleEndianBytes.HighByte(header), unchecked((byte)(item + 1)), 1, unchecked((byte)item), 0]);
        }
        records.AddRange([0x00, 0x00]);
        SeedRoomObjectPopulation(bus, population, records.ToArray());
        var foreground = new RoomTerrainBlock[size.Area];
        RoomLevelData level = CreateRoom(size, foreground, new RoomBlockBehavior[foreground.Length]);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 99, MaxHealth = 99 };
        var roomObjects = RoomObjectFixtures.Unbound();
        int loaded = RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria),
            RoomObjectPopulationImporter.Read(bus, population),
            new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertEqual(Kinds.Length, loaded, "five-item room population load count");

        roomObjects.Step(level);
        for (int item = 0; item < Kinds.Length; item++)
        {
            int blockIndex = size.RowMajorIndex(new Position16<RoomBlocks>((ushort)(item + 1), 1));
            AssertEqual(RoomBlockAppearance.Of(FirstPairFirstFrameBlock + item % CollectibleBlockPairs.InTurn.Count * 2),
                level.GetCollisionBlockByIndex(blockIndex).Terrain.Appearance,
                $"{Kinds[item]} writes its block pair's first-frame block into the terrain");
            AssertEqual(CollectibleCellPictures.DefinitionOf(new CollectiblePicture(Kinds[item], 0)),
                level.ForegroundPlane.Cells.Cells[blockIndex].Definition,
                $"{Kinds[item]} shows its own picture's first frame");
        }
        Console.WriteLine($"Collectible pictures: {Kinds.Length} items each show their own picture over the block pairs they take in turn.");
    }

    /// <summary>Runs the collectible picture check.</summary>
    [VerificationCommand("--collectible-pictures")]
    internal static int CollectiblePicturesCommand()
    {
        Suite(nameof(VerifyCollectiblePictures), VerifyCollectiblePictures);
        return 0;
    }
}
