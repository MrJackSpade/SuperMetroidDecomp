using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Checks room-actor artwork identity across all PLM domains.</summary>
internal static class RoomObjectContentIdentityVerification
{
    /// <summary>Selected-art verification only: constructed catalogs, no ROM and no gameplay traversal.</summary>
    internal static void VerifyObjectContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateObjectIdentityFixture();
        IReadOnlyDictionary<string, string> stock = RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet { ShotBlock = InstalledRoomObjectShotBlockVisuals.Stock(), GrappleBlock = InstalledRoomObjectGrappleBlockVisuals.Stock(), Station = InstalledRoomObjectStationVisuals.Stock(), BlueDoor = InstalledRoomObjectBlueDoorVisuals.Stock(), ColoredDoor = InstalledRoomObjectColoredDoorVisuals.Stock(), GreyDoor = InstalledRoomObjectGreyDoorVisuals.Stock(), EyeDoor = InstalledRoomObjectEyeDoorVisuals.Stock(), MotherBrainGlass = InstalledMotherBrainGlassFrames.Stock(), NoobTube = InstalledRoomObjectNoobTubeVisuals.Stock(), DownwardGate = InstalledRoomObjectDownwardGateVisuals.Stock(), ElevatorPlatform = RoomObjectElevatorPlatformVisualCatalog.Stock(), EscapeGate = InstalledEscapeGateFrames.Stock(), BombTorizoHand = RoomObjectBombTorizoHandVisualCatalog.Stock(), DraygonCannon = InstalledDraygonCannonFrames.Stock(), ChozoStatue = RoomObjectChozoStatueVisualCatalog.Stock(), LinkedRestore = InstalledRoomObjectLinkedRestoreVisuals.Stock(), TourianAccess = RoomObjectTourianAccessVisuals.Stock(), SpeedBooster = InstalledRoomObjectSpeedBoosterVisuals.Stock(), MaridiaElevatube = InstalledRoomObjectMaridiaElevatubeVisuals.Stock(), SporeSpawnCeiling = RoomObjectSporeSpawnCeilingVisualCatalog.Stock(), SamusEater = InstalledRoomObjectSamusEaterVisuals.Stock(), BotwoonWall = InstalledRoomObjectBotwoonWallVisuals.Stock(), Kraid = InstalledRoomObjectKraidVisuals.Stock(), Crocomire = RoomObjectCrocomireVisualCatalog.Stock(), MotherBrainFakeDeath = InstalledMotherBrainFakeDeathFrames.Stock(), Collectible = InstalledRoomObjectCollectibleVisuals.Stock(), RevealAndWall = InstalledRoomObjectRevealAndWallVisuals.Stock() });
        IReadOnlyDictionary<string, string> reordered = CreateObjectIdentityFixture(reverse: true);
        AssertEqual(27, baseline.Count, "all room-actor artwork domains");
        foreach ((string name, string hash) in baseline)
        {
            AssertEqual(stock[name], hash, name + " fixture matches the production stock selection");
            AssertEqual(hash, reordered[name], name + " ignores record insertion order");
        }

        RoomBlockAppearance two = RoomBlockAppearance.Of(2), three = RoomBlockAppearance.Of(3), four = RoomBlockAppearance.Of(4);
        var runs = new Dictionary<string, RoomBlockAppearance[][]> { ["frame"] = [[two, three], [four]] };
        var repartitioned = new Dictionary<string, RoomBlockAppearance[][]> { ["frame"] = [[two], [three, four]] };
        var swapped = new Dictionary<string, RoomBlockAppearance[][]> { ["frame"] = [[four], [two, three]] };
        AssertTrue(SelectedPresentationHash.FromAppearanceFrames("test", runs) !=
            SelectedPresentationHash.FromAppearanceFrames("test", repartitioned), "run boundaries are not flattened away");
        AssertTrue(SelectedPresentationHash.FromAppearanceFrames("test", runs) !=
            SelectedPresentationHash.FromAppearanceFrames("test", swapped), "native draw-run order contributes to identity");
        Console.WriteLine("  Room-actor content: all 27 domains, canonical ordering, " +
            "draw-run framing and host compatibility pass without a ROM.");
    }

    /// <summary>Builds stock PLM catalogs from the compiled definitions or the installed pictures, optionally in reversed insertion order.</summary>
    private static IReadOnlyDictionary<string, string> CreateObjectIdentityFixture(bool reverse = false)
    {
        // Compiled definitions determine the shape, IDs and appearances.
        IEnumerable<RoomObjectContentIdentityFrame> Runs(
            IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> definitions)
        {
            var frames = definitions.Select(draw => new RoomObjectContentIdentityFrame(draw.Draw, Runs:draw.Runs.Span
                .SelectToArray(run => run.Blocks.Span
                    .SelectToArray(block => block.Appearance)))).ToArray();
            return reverse ? frames.Reverse() : frames;
        }

        // The pictured families have no compiled appearance (#1557): they are rebuilt from their installed pictures.
        RoomObjectFrameImages<TFrame> Pictures<TFrame>(RoomObjectFrameImages<TFrame> installed) where TFrame : notnull
        {
            var frames = installed.All.Select(frame => KeyValuePair.Create(frame.Key, new RoomObjectFrameImage(
                (reverse ? frame.Value.Cells.Reverse() : frame.Value.Cells).ToDictionary(cell => cell.Key, cell => cell.Value)))).ToArray();
            return new RoomObjectFrameImages<TFrame>((reverse ? frames.Reverse() : frames).ToDictionary(frame => frame.Key, frame => frame.Value));
        }

        IEnumerable<RoomObjectContentIdentityDefinition> Words(
            IEnumerable<RoomObjectContentIdentityDefinition> definitions)
        {
            var frames = definitions.Select(draw =>
                new RoomObjectContentIdentityDefinition(draw.Draw, draw.Appearance)).ToArray();
            return reverse ? frames.Reverse() : frames;
        }

        return RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet { ShotBlock = RoomObjectShotBlockVisualFiles.Create(Runs(RoomObjectShotBlockVisualFiles.All)
                .Select(draw => new RoomObjectVisualRunEntry(RoomObjectShotBlockVisualFiles.VisualId(draw.Draw), draw.Runs))), GrappleBlock = RoomObjectGrappleBlockVisualFiles.Create(Words(RoomObjectGrappleBlockVisualFiles.All.Select(draw => new RoomObjectContentIdentityDefinition(RoomObjectDraw.Of(draw.Frame), draw.Terrain.Appearance)))
                .Select(draw => new RoomObjectVisualBlockEntry(RoomObjectGrappleBlockVisualFiles.VisualId(RoomObjectDraw.FrameOf<GrappleBlockDraw>(draw.Draw, "Grapple-block")), draw.Appearance))), Station = RoomObjectStationVisualFiles.Create(Runs(RoomObjectStationVisualFiles.All)
                .Select(draw => new RoomObjectVisualRunEntry(RoomObjectStationVisualFiles.VisualId(draw.Draw), draw.Runs))), BlueDoor = new InstalledRoomObjectBlueDoorVisuals(Pictures(InstalledRoomObjectBlueDoorVisuals.Stock().Images)), ColoredDoor = new InstalledRoomObjectColoredDoorVisuals(Pictures(InstalledRoomObjectColoredDoorVisuals.Stock().Images)), GreyDoor = new InstalledRoomObjectGreyDoorVisuals(Pictures(InstalledRoomObjectGreyDoorVisuals.Stock().Images)), EyeDoor = new InstalledRoomObjectEyeDoorVisuals(Pictures(InstalledRoomObjectEyeDoorVisuals.Stock().Images)), MotherBrainGlass = new InstalledMotherBrainGlassFrames(Pictures(InstalledMotherBrainGlassFrames.Stock().Images)), NoobTube = RoomObjectNoobTubeVisualFiles.Create(Runs(RoomObjectNoobTubeVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectNoobTubeVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DownwardGate = RoomObjectDownwardGateVisualFiles.Create(Runs(RoomObjectDownwardGateVisualFiles.All)
                .Select(draw => new RoomObjectVisualRunEntry(RoomObjectDownwardGateVisualFiles.VisualId(draw.Draw), draw.Runs))), ElevatorPlatform = RoomObjectElevatorPlatformVisualFiles.Create(Runs(RoomObjectElevatorPlatformVisualFiles.DrawLists)
                .Select(draw => new RoomObjectVisualRunEntry(RoomObjectElevatorPlatformVisualFiles.VisualId(draw.Draw), draw.Runs))), EscapeGate = new InstalledEscapeGateFrames(Pictures(InstalledEscapeGateFrames.Stock().Images)), BombTorizoHand = RoomObjectBombTorizoHandVisualFiles.Create(Runs(RoomObjectBombTorizoHandVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectBombTorizoHandVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DraygonCannon = new InstalledDraygonCannonFrames(Pictures(InstalledDraygonCannonFrames.Stock().Images)), ChozoStatue = RoomObjectChozoStatueVisualFiles.Create(Runs(RoomObjectChozoStatueVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectChozoStatueVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), LinkedRestore = RoomObjectLinkedRestoreVisualFiles.Create(Runs(RoomObjectLinkedRestoreVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectLinkedRestoreVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), TourianAccess = RoomObjectTourianAccessVisualFiles.Create(Runs(RoomObjectTourianAccessVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectTourianAccessVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SpeedBooster = RoomObjectSpeedBoosterVisualFiles.Create(Runs(RoomObjectSpeedBoosterVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectSpeedBoosterVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MaridiaElevatube = RoomObjectMaridiaElevatubeVisualFiles.Create(Runs(RoomObjectMaridiaElevatubeVisualFiles.AllDraws)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectMaridiaElevatubeVisualFiles.DrawVisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SporeSpawnCeiling = RoomObjectSporeSpawnCeilingVisualFiles.Create(Runs(RoomObjectSporeSpawnCeilingVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectSporeSpawnCeilingVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SamusEater = RoomObjectSamusEaterVisualFiles.Create(Runs(RoomObjectSamusEaterVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectSamusEaterVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BotwoonWall = RoomObjectBotwoonWallVisualFiles.Create(Runs(RoomObjectBotwoonWallVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectBotwoonWallVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Kraid = RoomObjectKraidVisualFiles.Create(Runs(RoomObjectKraidVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectKraidVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Crocomire = RoomObjectCrocomireVisualFiles.Create(Runs(RoomObjectCrocomireVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectCrocomireVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainFakeDeath = new InstalledMotherBrainFakeDeathFrames(Pictures(InstalledMotherBrainFakeDeathFrames.Stock().Images)), Collectible = RoomObjectCollectibleVisualFiles.Create(Words(RoomObjectCollectibleVisualFiles.Editable.Select(draw => new RoomObjectContentIdentityDefinition(RoomObjectDraw.Of(draw.Draw), draw.Terrain.Appearance)))
                .Select(draw => new RoomObjectVisualBlockEntry(RoomObjectCollectibleVisualFiles.VisualId(RoomObjectDraw.FrameOf<RoomObjectCollectibleDrawDefinitions.Draw>(draw.Draw, "collectible")), draw.Appearance))), RevealAndWall = RoomObjectRevealAndWallVisualFiles.Create(Runs(RoomObjectRevealAndWallVisualFiles.All)
                .Select(draw => new RoomObjectVisualEntry(RoomObjectRevealAndWallVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))) });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int RoomObjectContentIdentityCommand()
    {
        Suite(nameof(VerifyObjectContentIdentity), () => VerifyObjectContentIdentity());
        return 0;
    }
}
