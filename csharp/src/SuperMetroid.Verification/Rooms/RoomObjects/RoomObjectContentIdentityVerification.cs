using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Checks room-actor artwork identity across all PLM domains.</summary>
internal static class RoomObjectContentIdentityVerification
{
    /// <summary>Selected-art verification only: constructed catalogs, no ROM and no gameplay traversal.</summary>
    internal static void VerifyObjectContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateObjectIdentityFixture();
        IReadOnlyDictionary<string, string> stock = RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet { ShotBlock = RoomObjectPicturesStock.ShotBlock, GrappleBlock = RoomObjectPicturesStock.GrappleBlock, Station = RoomObjectPicturesStock.Station, BlueDoor = RoomObjectPicturesStock.BlueDoor, ColoredDoor = RoomObjectPicturesStock.ColoredDoor, GreyDoor = RoomObjectPicturesStock.GreyDoor, EyeDoor = RoomObjectPicturesStock.EyeDoor, MotherBrainGlass = InstalledMotherBrainGlassFrames.Stock(), NoobTube = RoomObjectPicturesStock.NoobTube, DownwardGate = RoomObjectPicturesStock.DownwardGate, ElevatorPlatform = RoomObjectPicturesStock.ElevatorPlatform, EscapeGate = RoomObjectPicturesStock.EscapeGate, BombTorizoHand = RoomObjectPicturesStock.BombTorizoHand, DraygonCannon = InstalledDraygonCannonFrames.Stock(), PhantoonDoor = InstalledPhantoonDoorFrames.Stock(), ChozoStatue = RoomObjectPicturesStock.ChozoStatue, LinkedRestore = RoomObjectPicturesStock.LinkedRestore, TourianAccess = RoomObjectTourianAccessVisuals.Stock(), SpeedBooster = RoomObjectPicturesStock.SpeedBooster, MaridiaElevatube = RoomObjectPicturesStock.MaridiaElevatube, SporeSpawnCeiling = RoomObjectPicturesStock.SporeSpawnCeiling, SamusEater = RoomObjectPicturesStock.SamusEater, BotwoonWall = RoomObjectPicturesStock.BotwoonWall, Kraid = RoomObjectPicturesStock.Kraid, Crocomire = RoomObjectPicturesStock.Crocomire, MotherBrainFakeDeath = InstalledMotherBrainFakeDeathFrames.Stock(), Collectible = RoomObjectPicturesStock.Collectible, RevealAndWall = RoomObjectPicturesStock.RevealAndWall, SetupBlocks = InstalledRoomObjectSetupBlocks.Stock() });
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

        return RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet { ShotBlock = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.ShotBlock.Images)), GrappleBlock = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.GrappleBlock.Images)), Station = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.Station.Images)), BlueDoor = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.BlueDoor.Images)), ColoredDoor = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.ColoredDoor.Images)), GreyDoor = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.GreyDoor.Images)), EyeDoor = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.EyeDoor.Images)), MotherBrainGlass = new InstalledMotherBrainGlassFrames(Pictures(InstalledMotherBrainGlassFrames.Stock().Images)), NoobTube = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.NoobTube.Images)), DownwardGate = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.DownwardGate.Images)), ElevatorPlatform = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.ElevatorPlatform.Images)), EscapeGate = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.EscapeGate.Images)), BombTorizoHand = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.BombTorizoHand.Images)), DraygonCannon = new InstalledDraygonCannonFrames(Pictures(InstalledDraygonCannonFrames.Stock().Images)), PhantoonDoor = new InstalledPhantoonDoorFrames(Pictures(InstalledPhantoonDoorFrames.Stock().Images)), ChozoStatue = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.ChozoStatue.Images)), LinkedRestore = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.LinkedRestore.Images)), TourianAccess = new RoomObjectTourianAccessVisuals(Pictures(RoomObjectTourianAccessVisuals.Stock().Images)), SpeedBooster = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.SpeedBooster.Images)), MaridiaElevatube = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.MaridiaElevatube.Images)), SporeSpawnCeiling = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.SporeSpawnCeiling.Images)), SamusEater = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.SamusEater.Images)), BotwoonWall = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.BotwoonWall.Images)), Kraid = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.Kraid.Images)), Crocomire = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.Crocomire.Images)), MotherBrainFakeDeath = new InstalledMotherBrainFakeDeathFrames(Pictures(InstalledMotherBrainFakeDeathFrames.Stock().Images)), Collectible = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.Collectible.Images)), RevealAndWall = new InstalledRoomObjectPictures(Pictures(RoomObjectPicturesStock.RevealAndWall.Images)), SetupBlocks = SetupBlocks() });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int RoomObjectContentIdentityCommand()
    {
        Suite(nameof(VerifyObjectContentIdentity), () => VerifyObjectContentIdentity());
        return 0;
    }
}
