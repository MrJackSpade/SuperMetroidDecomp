using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks, update by update, what a collectible shows as it plays its short animations and its two alternating looks
/// (#1994): a shot Chozo orb's burst, a shot block's reveal, the revealed item's looks until its block forms again over
/// it, and a collected item's block growing back.
/// </summary>
internal static class CollectibleTimelinesVerification
{
    /// <summary>The updates from a shot block's whole image to the revealed item: three reveal images, four updates each.</summary>
    private const int RevealUpdates = 3 * CollectibleTimingDefinitions.RevealImageUpdates;

    /// <summary>Plays each animation through and compares the block's picture after every update with the authored timeline.</summary>
    internal static void VerifyCollectibleTimelines()
    {
        var bus = new TestAddressSpace();
        SeedCollectibleRom(bus);
        InstalledRoomObjectCollectibleVisuals collectibles = InstalledRoomObjectCollectibleVisuals.Stock();
        InstalledRoomObjectShotBlockVisuals shotBlocks = InstalledRoomObjectShotBlockVisuals.Stock();
        RoomBlockAppearance Collectible(CollectibleDraw draw) => collectibles.Image(CollectibleObjectDraws.Family.Of(draw)).At(Offset16<RoomBlocks>.Zero);
        RoomBlockAppearance Breakup(BlockBreakupStage stage) =>
            shotBlocks.Image(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, stage)).At(Offset16<RoomBlocks>.Zero);

        // A shot Chozo orb shows the empty block, its burst and the empty block, three updates each, then its item's
        // first look.
        CollectibleFixture orb = LoadCollectible(bus, header: RoomObjectNativeWords.Word(RoomObjectKind.ChozoBombs), roomArgument: 79, precollected: false);
        orb.RoomObjects.Step(orb.Level);
        orb.RoomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(orb.Block);
        var burst = new List<RoomBlockAppearance>();
        burst.AddRange(Enumerable.Repeat(Collectible(CollectibleDraw.Empty), CollectibleTimingDefinitions.OrbBurstImageUpdates));
        burst.AddRange(Enumerable.Repeat(Collectible(CollectibleDraw.ChozoOrbBurst), CollectibleTimingDefinitions.OrbBurstImageUpdates));
        burst.AddRange(Enumerable.Repeat(Collectible(CollectibleDraw.Empty), CollectibleTimingDefinitions.OrbBurstImageUpdates));
        AssertShows(orb, burst, "Chozo orb burst");
        orb.RoomObjects.Step(orb.Level);
        AssertEqual(CollectiblePhase.Visible, orb.RoomObjects.CollectiblePhases[0], "the burst ends with the item exposed");
        AssertEqual(RoomCellPicture.Of(new CollectiblePicture(InWorldCollectibleKind.Bombs, CollectibleLook.Primary)),
            orb.Level.ForegroundPlane.Cells.Cells[orb.BlockIndex].Picture, "the exposed item shows its first look");

        // A shot block crumbles from whole to dust, four updates each, then the Missile Tank shows its two looks, four
        // updates each, twenty-two times; its block forms again from dust, four updates each, and returns whole.
        CollectibleFixture shot = LoadCollectible(bus, header: unchecked(0xef7f + (int)InWorldCollectibleKind.MissileTank * 4), roomArgument: 150, precollected: false);
        RoomTerrainBlock concealed = shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).Terrain;
        shot.RoomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(shot.Block);
        var reveal = new List<RoomBlockAppearance>();
        foreach (CollectibleDraw draw in new[] { CollectibleDraw.ShotBlockIntact, CollectibleDraw.ShotBlockCracking, CollectibleDraw.ShotBlockDust })
            reveal.AddRange(Enumerable.Repeat(Collectible(draw), CollectibleTimingDefinitions.RevealImageUpdates));
        for (int pair = 0; pair < CollectibleTimingDefinitions.RevealedImagePairs; pair++)
        {
            reveal.AddRange(Enumerable.Repeat(Collectible(CollectibleDraw.MissileTankDim), CollectibleTimingDefinitions.VisibleImageUpdates));
            reveal.AddRange(Enumerable.Repeat(Collectible(CollectibleDraw.MissileTankBright), CollectibleTimingDefinitions.VisibleImageUpdates));
        }
        foreach (CollectibleDraw draw in new[] { CollectibleDraw.ShotBlockDust, CollectibleDraw.ShotBlockCracking })
            reveal.AddRange(Enumerable.Repeat(Collectible(draw), CollectibleTimingDefinitions.RevealImageUpdates));
        AssertShows(shot, reveal, "shot block reveal and hiding");
        shot.RoomObjects.Step(shot.Level);
        AssertEqual(concealed, shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).Terrain, "the hidden item's block returns as it was");
        AssertEqual(CollectiblePhase.ShotBlock, shot.RoomObjects.CollectiblePhases[0], "the hidden item waits for another shot");

        // A collected item's block stays empty for its time, then grows back from dust to rock, four updates each, and
        // returns as it was.
        shot.RoomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(shot.Block);
        StepFrames(RevealUpdates + 1, _ => shot.RoomObjects.Step(shot.Level));
        AssertTrue(shot.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(shot.Block), "the revealed item accepts Samus's touch");
        shot.RoomObjects.Step(shot.Level);
        shot.RoomObjects.CollectibleObjects.CompleteCollectibleMessage(shot.Level);
        var respawn = new List<RoomBlockAppearance>();
        respawn.AddRange(Enumerable.Repeat(Collectible(CollectibleDraw.Empty), CollectibleTimingDefinitions.CollectedShotBlockEmptyUpdates - 1));
        foreach (BlockBreakupStage stage in new[] { BlockBreakupStage.Dust, BlockBreakupStage.Pieces, BlockBreakupStage.Rock })
            respawn.AddRange(Enumerable.Repeat(Breakup(stage), CollectibleTimingDefinitions.RespawnImageUpdates));
        AssertShows(shot, respawn, "collected shot block respawn");
        shot.RoomObjects.Step(shot.Level);
        AssertEqual(concealed, shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).Terrain, "the collected item's block returns as it was");
        AssertEqual(CollectiblePhase.CollectedShotBlock, shot.RoomObjects.CollectiblePhases[0], "the collected item's block waits for another shot");
        Console.WriteLine("  Collectible timelines: the orb's burst, the shot block's reveal, hiding and respawn show each image for its updates.");
    }

    /// <summary>Steps <paramref name="fixture"/> once per expected picture and compares the item's block after each update.</summary>
    private static void AssertShows(CollectibleFixture fixture, List<RoomBlockAppearance> expected, string what)
    {
        for (int update = 0; update < expected.Count; update++)
        {
            fixture.RoomObjects.Step(fixture.Level);
            AssertEqual(expected[update], fixture.Level.GetCollisionBlockByIndex(fixture.BlockIndex).Terrain.Appearance,
                $"{what} update {update + 1}");
        }
    }

    /// <summary>Runs the collectible timeline checks.</summary>
    [VerificationCommand("--collectible-timelines")]
    internal static int CollectibleTimelinesCommand()
    {
        Suite(nameof(VerifyCollectibleTimelines), () => VerifyCollectibleTimelines());
        return 0;
    }
}
