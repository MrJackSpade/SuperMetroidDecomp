using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
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

        return RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = new InstalledRoomPlmShotBlockVisuals(Runs(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName, ShotBlockPlmDraws.All)
                .Select(draw => new RoomPlmVisualRunEntry(ShotBlockPlmDraws.VisualId(draw.Draw), draw.Runs))), GrappleBlock = new InstalledRoomPlmGrappleBlockVisuals(Words(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
                GrappleBlockPlmDraws.All.Select(draw => new PlmContentIdentityDefinition(PlmDraw.Of(draw.Frame), draw.Terrain.Appearance)))
                .Select(draw => new RoomPlmVisualBlockEntry(GrappleBlockPlmDraws.VisualId(PlmDraw.FrameOf<GrappleBlockDraw>(draw.Draw, "Grapple-block")), draw.Appearance))), Station = new InstalledRoomPlmStationVisuals(Runs(GameInstallationLayout.RoomPlmStationVisualDirectoryName, StationPlmDraws.All)
                .Select(draw => new RoomPlmVisualRunEntry(StationPlmDraws.VisualId(draw.Draw), draw.Runs))), BlueDoor = new InstalledRoomPlmBlueDoorVisuals(Runs(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName, BlueDoorCaps.Editable)
                .Select(draw => new RoomPlmVisualEntry(BlueDoorCaps.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), ColoredDoor = new InstalledRoomPlmColoredDoorVisuals(Runs(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName, ColoredDoorCaps.All)
                .Select(draw => new RoomPlmVisualEntry(ColoredDoorCaps.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), GreyDoor = new InstalledRoomPlmGreyDoorVisuals(Runs(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName, GreyDoorCaps.All)
                .Select(draw => new RoomPlmVisualEntry(GreyDoorCaps.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), EyeDoor = new InstalledRoomPlmEyeDoorVisuals(Runs(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName, EyeDoorDraws.Editable)
                .Select(draw => new RoomPlmVisualEntry(EyeDoorDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainGlass = new RoomPlmMotherBrainGlassVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName, MotherBrainGlassDraws.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainGlassDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), NoobTube = new InstalledRoomPlmNoobTubeVisuals(Runs(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName, NoobTubePlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(NoobTubePlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DownwardGate = new InstalledRoomPlmDownwardGateVisuals(Runs(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName, DownwardGatePlmDraws.All)
                .Select(draw => new RoomPlmVisualRunEntry(DownwardGatePlmDraws.VisualId(draw.Draw), draw.Runs))), ElevatorPlatform = new RoomPlmElevatorPlatformVisualCatalog(Runs(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName, ElevatorPlatformDraws.DrawLists)
                .Select(draw => new RoomPlmVisualRunEntry(ElevatorPlatformDraws.VisualId(draw.Draw), draw.Runs))), EscapeGate = new RoomPlmEscapeGateVisualCatalog(Runs(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName, MotherBrainEscapeGateDraws.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainEscapeGateDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BombTorizoHand = new RoomPlmBombTorizoHandVisualCatalog(Runs(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName, BombTorizoHandDraws.All)
                .Select(draw => new RoomPlmVisualEntry(BombTorizoHandDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DraygonCannon = new RoomPlmDraygonCannonVisualCatalog(Runs(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName, DraygonCannonDraws.All)
                .Select(draw => new RoomPlmVisualEntry(DraygonCannonDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), ChozoStatue = new RoomPlmChozoStatueVisualCatalog(Runs(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName, ChozoStatueDraws.All)
                .Select(draw => new RoomPlmVisualEntry(ChozoStatueDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), LinkedRestore = new InstalledRoomPlmLinkedRestoreVisuals(Runs(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName, RoomPlmLinkedRestoreDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(LinkedRestorePlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), TourianAccess = new RoomPlmTourianAccessVisuals(Runs(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName, TourianAccessDraws.All)
                .Select(draw => new RoomPlmVisualEntry(TourianAccessDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SpeedBooster = new InstalledRoomPlmSpeedBoosterVisuals(Runs(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName, SpeedBoosterBlockPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(SpeedBoosterBlockPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MaridiaElevatube = new InstalledRoomPlmMaridiaElevatubeVisuals(Runs(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName, MaridiaElevatubePlmDefinitions.AllDraws)
                .Select(draw => new RoomPlmVisualEntry(MaridiaElevatubeDraws.DrawVisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SporeSpawnCeiling = new RoomPlmSporeSpawnCeilingVisualCatalog(Runs(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName, SporeSpawnCeilingDraws.All)
                .Select(draw => new RoomPlmVisualEntry(SporeSpawnCeilingDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SamusEater = new InstalledRoomPlmSamusEaterVisuals(Runs(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName, SamusEaterPlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(SamusEaterPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BotwoonWall = new InstalledRoomPlmBotwoonWallVisuals(Runs(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName, BotwoonWallPlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(BotwoonWallPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Kraid = new InstalledRoomPlmKraidVisuals(Runs(GameInstallationLayout.RoomPlmKraidVisualDirectoryName, KraidRoomPlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(KraidRoomPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Crocomire = new RoomPlmCrocomireVisualCatalog(Runs(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName, CrocomireArenaDraws.All)
                .Select(draw => new RoomPlmVisualEntry(CrocomireArenaDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainFakeDeath = new RoomPlmMotherBrainFakeDeathVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName, MotherBrainFakeDeathDraws.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainFakeDeathDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Collectible = new InstalledRoomPlmCollectibleVisuals(Words(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
                CollectiblePlmDraws.All.Select(draw => new PlmContentIdentityDefinition(PlmDraw.Of(draw.Draw), draw.Terrain.Appearance)))
                .Select(draw => new RoomPlmVisualBlockEntry(CollectiblePlmDraws.VisualId(PlmDraw.FrameOf<RoomPlmCollectibleDrawDefinitions.Draw>(draw.Draw, "collectible")), draw.Appearance))) });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int PlmContentIdentityCommand()
    {
        Suite(nameof(VerifyPlmContentIdentity), () => VerifyPlmContentIdentity());
        return 0;
    }
}
