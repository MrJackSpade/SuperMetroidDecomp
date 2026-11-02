using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Checks that room-actor artwork identity tracks selected visuals independently across all PLM domains.</summary>
internal static class PlmContentIdentityVerification
{
    /// <summary>Selected-art verification only: constructed catalogs, no ROM and no gameplay traversal.</summary>
    internal static void VerifyPlmContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreatePlmIdentityFixture();
        IReadOnlyDictionary<string, string> stock = RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = InstalledRoomPlmShotBlockVisuals.Stock(), GrappleBlock = InstalledRoomPlmGrappleBlockVisuals.Stock(), Station = InstalledRoomPlmStationVisuals.Stock(), BlueDoor = InstalledRoomPlmBlueDoorVisuals.Stock(), ColoredDoor = InstalledRoomPlmColoredDoorVisuals.Stock(), GreyDoor = InstalledRoomPlmGreyDoorVisuals.Stock(), EyeDoor = InstalledRoomPlmEyeDoorVisuals.Stock(), MotherBrainGlass = RoomPlmMotherBrainGlassVisualCatalog.Stock(), NoobTube = InstalledRoomPlmNoobTubeVisuals.Stock(), DownwardGate = InstalledRoomPlmDownwardGateVisuals.Stock(), ElevatorPlatform = RoomPlmElevatorPlatformVisualCatalog.Stock(), EscapeGate = RoomPlmEscapeGateVisualCatalog.Stock(), BombTorizoHand = RoomPlmBombTorizoHandVisualCatalog.Stock(), DraygonCannon = RoomPlmDraygonCannonVisualCatalog.Stock(), ChozoStatue = RoomPlmChozoStatueVisualCatalog.Stock(), LinkedRestore = InstalledRoomPlmLinkedRestoreVisuals.Stock(), TourianAccess = RoomPlmTourianAccessVisuals.Stock(), SpeedBooster = InstalledRoomPlmSpeedBoosterVisuals.Stock(), MaridiaElevatube = InstalledRoomPlmMaridiaElevatubeVisuals.Stock(), SporeSpawnCeiling = RoomPlmSporeSpawnCeilingVisualCatalog.Stock(), SamusEater = InstalledRoomPlmSamusEaterVisuals.Stock(), BotwoonWall = InstalledRoomPlmBotwoonWallVisuals.Stock(), Kraid = InstalledRoomPlmKraidVisuals.Stock(), Crocomire = RoomPlmCrocomireVisualCatalog.Stock(), MotherBrainFakeDeath = RoomPlmMotherBrainFakeDeathVisualCatalog.Stock(), Collectible = InstalledRoomPlmCollectibleVisuals.Stock() });
        IReadOnlyDictionary<string, string> reordered = CreatePlmIdentityFixture(reverse: true);
        AssertEqual(26, baseline.Count, "all room-actor artwork domains");
        foreach ((string name, string hash) in baseline)
        {
            AssertEqual(stock[name], hash, name + " fixture matches the production stock selection");
            AssertEqual(hash, reordered[name], name + " ignores record insertion order");
        }

        foreach (string domain in baseline.Keys)
        {
            IReadOnlyDictionary<string, string> changed = CreatePlmIdentityFixture(domain);
            AssertTrue(baseline[domain] != changed[domain], domain + " detects a selected visual edit");
            foreach (string other in baseline.Keys.Where(name => name != domain))
                AssertEqual(baseline[other], changed[other], domain + " preserves unrelated " + other);
            var before = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, baseline);
            var after = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, changed);
            AssertTrue(before.CompositeSha256 != after.CompositeSha256, domain + " changes the host identity");
            AssertTrue(before.GetCompatibilityWarnings(after.ToSnapshot(), "test").Single()
                .Contains(domain, StringComparison.Ordinal), domain + " has a specific compatibility warning");
        }

        RoomBlockAppearance two = RoomBlockAppearance.Of(2), three = RoomBlockAppearance.Of(3), four = RoomBlockAppearance.Of(4);
        var runs = new Dictionary<string, RoomBlockAppearance[][]> { ["frame"] = [[two, three], [four]] };
        var repartitioned = new Dictionary<string, RoomBlockAppearance[][]> { ["frame"] = [[two], [three, four]] };
        var swapped = new Dictionary<string, RoomBlockAppearance[][]> { ["frame"] = [[four], [two, three]] };
        AssertTrue(SelectedPresentationHash.FromAppearanceFrames("test", runs) !=
            SelectedPresentationHash.FromAppearanceFrames("test", repartitioned), "run boundaries are not flattened away");
        AssertTrue(SelectedPresentationHash.FromAppearanceFrames("test", runs) !=
            SelectedPresentationHash.FromAppearanceFrames("test", swapped), "native draw-run order contributes to identity");
        Console.WriteLine("  Room-actor content: all 26 domains, canonical ordering, independent edits, " +
            "draw-run framing and host compatibility pass without a ROM.");
    }

    /// <summary>Builds stock-shaped PLM catalogs with one optional visual mutation and optional reversed insertion order.</summary>
    private static IReadOnlyDictionary<string, string> CreatePlmIdentityFixture(
        string? edited = null, bool reverse = false)
    {
        // Compiled definitions determine only the required shape/IDs. Copy appearances before editing;
        // the terrain, run geometry and programs must remain untouched.
        IEnumerable<PlmContentIdentityFrame> Runs(string domain,
            IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions)
        {
            var frames = definitions.Select(draw => new PlmContentIdentityFrame(draw.Draw, Runs:draw.Runs.Span
                .SelectToArray(run => run.Blocks.Span
                    .SelectToArray(block => block.Appearance)))).ToArray();
            if (domain == edited)
                foreach (var frame in frames)
                    frame.Runs[^1][^1] = frame.Runs[^1][^1].WithBlock(frame.Runs[^1][^1].Block ^ 1);
            return reverse ? frames.Reverse() : frames;
        }

        IEnumerable<PlmContentIdentityDefinition> Words(string domain,
            IEnumerable<PlmContentIdentityDefinition> definitions)
        {
            var frames = definitions.Select(draw =>
                new PlmContentIdentityDefinition(draw.Draw, domain == edited
                    ? draw.Appearance.WithBlock(draw.Appearance.Block ^ 1) : draw.Appearance)).ToArray();
            return reverse ? frames.Reverse() : frames;
        }

        return RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = RoomPlmShotBlockVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName, RoomPlmShotBlockVisualFiles.All)
                .Select(draw => new RoomPlmVisualRunEntry(RoomPlmShotBlockVisualFiles.VisualId(draw.Draw), draw.Runs))), GrappleBlock = RoomPlmGrappleBlockVisualFiles.Create(Words(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
                RoomPlmGrappleBlockVisualFiles.All.Select(draw => new PlmContentIdentityDefinition(PlmDraw.Of(draw.Frame), draw.Terrain.Appearance)))
                .Select(draw => new RoomPlmVisualBlockEntry(RoomPlmGrappleBlockVisualFiles.VisualId(PlmDraw.FrameOf<GrappleBlockDraw>(draw.Draw, "Grapple-block")), draw.Appearance))), Station = RoomPlmStationVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmStationVisualDirectoryName, RoomPlmStationVisualFiles.All)
                .Select(draw => new RoomPlmVisualRunEntry(RoomPlmStationVisualFiles.VisualId(draw.Draw), draw.Runs))), BlueDoor = RoomPlmBlueDoorVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName, RoomPlmBlueDoorVisualFiles.Editable)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmBlueDoorVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), ColoredDoor = RoomPlmColoredDoorVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName, RoomPlmColoredDoorVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmColoredDoorVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), GreyDoor = RoomPlmGreyDoorVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName, RoomPlmGreyDoorVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmGreyDoorVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), EyeDoor = RoomPlmEyeDoorVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName, RoomPlmEyeDoorVisualFiles.Editable)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmEyeDoorVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainGlass = RoomPlmMotherBrainGlassVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName, RoomPlmMotherBrainGlassVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmMotherBrainGlassVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), NoobTube = RoomPlmNoobTubeVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName, RoomPlmNoobTubeVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmNoobTubeVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DownwardGate = RoomPlmDownwardGateVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName, RoomPlmDownwardGateVisualFiles.All)
                .Select(draw => new RoomPlmVisualRunEntry(RoomPlmDownwardGateVisualFiles.VisualId(draw.Draw), draw.Runs))), ElevatorPlatform = RoomPlmElevatorPlatformVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName, RoomPlmElevatorPlatformVisualFiles.DrawLists)
                .Select(draw => new RoomPlmVisualRunEntry(RoomPlmElevatorPlatformVisualFiles.VisualId(draw.Draw), draw.Runs))), EscapeGate = RoomPlmEscapeGateVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName, RoomPlmEscapeGateVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmEscapeGateVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BombTorizoHand = RoomPlmBombTorizoHandVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName, RoomPlmBombTorizoHandVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmBombTorizoHandVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DraygonCannon = RoomPlmDraygonCannonVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName, RoomPlmDraygonCannonVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmDraygonCannonVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), ChozoStatue = RoomPlmChozoStatueVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName, RoomPlmChozoStatueVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmChozoStatueVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), LinkedRestore = RoomPlmLinkedRestoreVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName, RoomPlmLinkedRestoreVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmLinkedRestoreVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), TourianAccess = RoomPlmTourianAccessVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName, RoomPlmTourianAccessVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmTourianAccessVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SpeedBooster = RoomPlmSpeedBoosterVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName, RoomPlmSpeedBoosterVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmSpeedBoosterVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MaridiaElevatube = RoomPlmMaridiaElevatubeVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName, RoomPlmMaridiaElevatubeVisualFiles.AllDraws)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmMaridiaElevatubeVisualFiles.DrawVisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SporeSpawnCeiling = RoomPlmSporeSpawnCeilingVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName, RoomPlmSporeSpawnCeilingVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmSporeSpawnCeilingVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SamusEater = RoomPlmSamusEaterVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName, RoomPlmSamusEaterVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmSamusEaterVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BotwoonWall = RoomPlmBotwoonWallVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName, RoomPlmBotwoonWallVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmBotwoonWallVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Kraid = RoomPlmKraidVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmKraidVisualDirectoryName, RoomPlmKraidVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmKraidVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Crocomire = RoomPlmCrocomireVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName, RoomPlmCrocomireVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmCrocomireVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainFakeDeath = RoomPlmMotherBrainFakeDeathVisualFiles.Create(Runs(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName, RoomPlmMotherBrainFakeDeathVisualFiles.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmMotherBrainFakeDeathVisualFiles.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Collectible = RoomPlmCollectibleVisualFiles.Create(Words(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
                RoomPlmCollectibleVisualFiles.All.Select(draw => new PlmContentIdentityDefinition(PlmDraw.Of(draw.Draw), draw.Terrain.Appearance)))
                .Select(draw => new RoomPlmVisualBlockEntry(RoomPlmCollectibleVisualFiles.VisualId(PlmDraw.FrameOf<RoomPlmCollectibleDrawDefinitions.Draw>(draw.Draw, "collectible")), draw.Appearance))) });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int PlmContentIdentityCommand()
    {
        Suite(nameof(VerifyPlmContentIdentity), () => VerifyPlmContentIdentity());
        return 0;
    }
}
