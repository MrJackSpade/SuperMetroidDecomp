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
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Checks room-actor artwork identity across all PLM domains.</summary>
internal static class RoomObjectContentIdentityVerification
{
    /// <summary>Selected-art verification only: constructed catalogs, no ROM and no gameplay traversal.</summary>
    internal static void VerifyObjectContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateObjectIdentityFixture();
        IReadOnlyDictionary<string, string> stock = RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet { ShotBlock = InstalledRoomObjectShotBlockVisuals.Stock(), GrappleBlock = InstalledRoomObjectGrappleBlockVisuals.Stock(), Station = InstalledRoomObjectStationVisuals.Stock(), BlueDoor = InstalledRoomObjectBlueDoorVisuals.Stock(), ColoredDoor = InstalledRoomObjectColoredDoorVisuals.Stock(), GreyDoor = InstalledRoomObjectGreyDoorVisuals.Stock(), EyeDoor = InstalledRoomObjectEyeDoorVisuals.Stock(), MotherBrainGlass = InstalledMotherBrainGlassFrames.Stock(), NoobTube = InstalledRoomObjectNoobTubeVisuals.Stock(), DownwardGate = InstalledRoomObjectDownwardGateVisuals.Stock(), ElevatorPlatform = InstalledRoomObjectElevatorPlatformVisuals.Stock(), EscapeGate = InstalledEscapeGateFrames.Stock(), BombTorizoHand = InstalledRoomObjectBombTorizoHandVisuals.Stock(), DraygonCannon = InstalledDraygonCannonFrames.Stock(), PhantoonDoor = InstalledPhantoonDoorFrames.Stock(), ChozoStatue = InstalledRoomObjectChozoStatueVisuals.Stock(), LinkedRestore = InstalledRoomObjectLinkedRestoreVisuals.Stock(), TourianAccess = RoomObjectTourianAccessVisuals.Stock(), SpeedBooster = InstalledRoomObjectSpeedBoosterVisuals.Stock(), MaridiaElevatube = InstalledRoomObjectMaridiaElevatubeVisuals.Stock(), SporeSpawnCeiling = InstalledRoomObjectSporeSpawnCeilingVisuals.Stock(), SamusEater = InstalledRoomObjectSamusEaterVisuals.Stock(), BotwoonWall = InstalledRoomObjectBotwoonWallVisuals.Stock(), Kraid = InstalledRoomObjectKraidVisuals.Stock(), Crocomire = InstalledRoomObjectCrocomireVisuals.Stock(), MotherBrainFakeDeath = InstalledMotherBrainFakeDeathFrames.Stock(), Collectible = InstalledRoomObjectCollectibleVisuals.Stock(), RevealAndWall = InstalledRoomObjectRevealAndWallVisuals.Stock(), SetupBlocks = InstalledRoomObjectSetupBlocks.Stock() });
        IReadOnlyDictionary<string, string> reordered = CreateObjectIdentityFixture(reverse: true);
        AssertEqual(29, baseline.Count, "all room-actor artwork domains");
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
        Console.WriteLine("  Room-actor content: all 29 domains, canonical ordering, " +
            "draw-run framing and host compatibility pass without a ROM.");
    }

    /// <summary>Rebuilds every family's installed pictures, optionally with their frames and cells in reversed insertion order (#1557).</summary>
    private static IReadOnlyDictionary<string, string> CreateObjectIdentityFixture(bool reverse = false)
    {
        // Every family is pictured (#1557): its pictures are rebuilt from the installed ones.
        RoomObjectFrameImages<TFrame> Pictures<TFrame>(RoomObjectFrameImages<TFrame> installed) where TFrame : notnull
        {
            var frames = installed.All.Select(frame => KeyValuePair.Create(frame.Key, new RoomObjectFrameImage(
                (reverse ? frame.Value.Cells.Reverse() : frame.Value.Cells).ToDictionary(cell => cell.Key, cell => cell.Value)))).ToArray();
            return new RoomObjectFrameImages<TFrame>((reverse ? frames.Reverse() : frames).ToDictionary(frame => frame.Key, frame => frame.Value));
        }

        // The setup blocks are rebuilt from the installed ones, in reversed insertion order when asked (#1580).
        InstalledRoomObjectSetupBlocks SetupBlocks()
        {
            KeyValuePair<RoomObjectSetupBlock, RoomBlockAppearance>[] blocks = [.. InstalledRoomObjectSetupBlocks.Stock().All];
            return new InstalledRoomObjectSetupBlocks((reverse ? blocks.Reverse() : blocks).ToDictionary(block => block.Key, block => block.Value));
        }

        return RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet { ShotBlock = new InstalledRoomObjectShotBlockVisuals(Pictures(InstalledRoomObjectShotBlockVisuals.Stock().Images)), GrappleBlock = new InstalledRoomObjectGrappleBlockVisuals(Pictures(InstalledRoomObjectGrappleBlockVisuals.Stock().Images)), Station = new InstalledRoomObjectStationVisuals(Pictures(InstalledRoomObjectStationVisuals.Stock().Images)), BlueDoor = new InstalledRoomObjectBlueDoorVisuals(Pictures(InstalledRoomObjectBlueDoorVisuals.Stock().Images)), ColoredDoor = new InstalledRoomObjectColoredDoorVisuals(Pictures(InstalledRoomObjectColoredDoorVisuals.Stock().Images)), GreyDoor = new InstalledRoomObjectGreyDoorVisuals(Pictures(InstalledRoomObjectGreyDoorVisuals.Stock().Images)), EyeDoor = new InstalledRoomObjectEyeDoorVisuals(Pictures(InstalledRoomObjectEyeDoorVisuals.Stock().Images)), MotherBrainGlass = new InstalledMotherBrainGlassFrames(Pictures(InstalledMotherBrainGlassFrames.Stock().Images)), NoobTube = new InstalledRoomObjectNoobTubeVisuals(Pictures(InstalledRoomObjectNoobTubeVisuals.Stock().Images)), DownwardGate = new InstalledRoomObjectDownwardGateVisuals(Pictures(InstalledRoomObjectDownwardGateVisuals.Stock().Images)), ElevatorPlatform = new InstalledRoomObjectElevatorPlatformVisuals(Pictures(InstalledRoomObjectElevatorPlatformVisuals.Stock().Images)), EscapeGate = new InstalledEscapeGateFrames(Pictures(InstalledEscapeGateFrames.Stock().Images)), BombTorizoHand = new InstalledRoomObjectBombTorizoHandVisuals(Pictures(InstalledRoomObjectBombTorizoHandVisuals.Stock().Images)), DraygonCannon = new InstalledDraygonCannonFrames(Pictures(InstalledDraygonCannonFrames.Stock().Images)), PhantoonDoor = new InstalledPhantoonDoorFrames(Pictures(InstalledPhantoonDoorFrames.Stock().Images)), ChozoStatue = new InstalledRoomObjectChozoStatueVisuals(Pictures(InstalledRoomObjectChozoStatueVisuals.Stock().Images)), LinkedRestore = new InstalledRoomObjectLinkedRestoreVisuals(Pictures(InstalledRoomObjectLinkedRestoreVisuals.Stock().Images)), TourianAccess = new RoomObjectTourianAccessVisuals(Pictures(RoomObjectTourianAccessVisuals.Stock().Images)), SpeedBooster = new InstalledRoomObjectSpeedBoosterVisuals(Pictures(InstalledRoomObjectSpeedBoosterVisuals.Stock().Images)), MaridiaElevatube = new InstalledRoomObjectMaridiaElevatubeVisuals(Pictures(InstalledRoomObjectMaridiaElevatubeVisuals.Stock().Images)), SporeSpawnCeiling = new InstalledRoomObjectSporeSpawnCeilingVisuals(Pictures(InstalledRoomObjectSporeSpawnCeilingVisuals.Stock().Images)), SamusEater = new InstalledRoomObjectSamusEaterVisuals(Pictures(InstalledRoomObjectSamusEaterVisuals.Stock().Images)), BotwoonWall = new InstalledRoomObjectBotwoonWallVisuals(Pictures(InstalledRoomObjectBotwoonWallVisuals.Stock().Images)), Kraid = new InstalledRoomObjectKraidVisuals(Pictures(InstalledRoomObjectKraidVisuals.Stock().Images)), Crocomire = new InstalledRoomObjectCrocomireVisuals(Pictures(InstalledRoomObjectCrocomireVisuals.Stock().Images)), MotherBrainFakeDeath = new InstalledMotherBrainFakeDeathFrames(Pictures(InstalledMotherBrainFakeDeathFrames.Stock().Images)), Collectible = new InstalledRoomObjectCollectibleVisuals(Pictures(InstalledRoomObjectCollectibleVisuals.Stock().Images)), RevealAndWall = new InstalledRoomObjectRevealAndWallVisuals(Pictures(InstalledRoomObjectRevealAndWallVisuals.Stock().Images)), SetupBlocks = SetupBlocks() });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int RoomObjectContentIdentityCommand()
    {
        Suite(nameof(VerifyObjectContentIdentity), () => VerifyObjectContentIdentity());
        return 0;
    }
}
