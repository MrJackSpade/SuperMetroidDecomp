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
        IReadOnlyDictionary<string, string> stock = RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = InstalledRoomPlmShotBlockVisuals.Stock(), GrappleBlock = InstalledRoomPlmGrappleBlockVisuals.Stock(), Station = InstalledRoomPlmStationVisuals.Stock(), BlueDoor = InstalledRoomPlmBlueDoorVisuals.Stock(), ColoredDoor = InstalledRoomPlmColoredDoorVisuals.Stock(), GreyDoor = InstalledRoomPlmGreyDoorVisuals.Stock(), EyeDoor = InstalledRoomPlmEyeDoorVisuals.Stock(), MotherBrainGlass = RoomPlmMotherBrainGlassVisualCatalog.Stock(), NoobTube = InstalledRoomPlmNoobTubeVisuals.Stock(), DownwardGate = InstalledRoomPlmDownwardGateVisuals.Stock(), ElevatorPlatform = RoomPlmElevatorPlatformVisualCatalog.Stock(), EscapeGate = RoomPlmEscapeGateVisualCatalog.Stock(), BombTorizoHand = RoomPlmBombTorizoHandVisualCatalog.Stock(), DraygonCannon = RoomPlmDraygonCannonVisualCatalog.Stock(), ChozoStatue = RoomPlmChozoStatueVisualCatalog.Stock(), LinkedRestore = InstalledRoomPlmLinkedRestoreVisuals.Stock(), TourianAccess = RoomPlmTourianAccessVisualCatalog.Stock(), SpeedBooster = InstalledRoomPlmSpeedBoosterVisuals.Stock(), MaridiaElevatube = InstalledRoomPlmMaridiaElevatubeVisuals.Stock(), SporeSpawnCeiling = RoomPlmSporeSpawnCeilingVisualCatalog.Stock(), SamusEater = InstalledRoomPlmSamusEaterVisuals.Stock(), BotwoonWall = InstalledRoomPlmBotwoonWallVisuals.Stock(), Kraid = InstalledRoomPlmKraidVisuals.Stock(), Crocomire = RoomPlmCrocomireVisualCatalog.Stock(), MotherBrainFakeDeath = RoomPlmMotherBrainFakeDeathVisualCatalog.Stock(), Collectible = InstalledRoomPlmCollectibleVisuals.Stock(), DynamicCollectibleArt = InstalledDynamicCollectibleArt.Stock() });
        IReadOnlyDictionary<string, string> reordered = CreatePlmIdentityFixture(reverse: true);
        AssertEqual(27, baseline.Count, "all room-actor artwork domains");
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
        AssertTrue(baseline[GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName] !=
            CreatePlmIdentityFixture("dynamic-palettes")[GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName],
            "dynamic item palette selectors contribute independently of pixels");

        var runs = new Dictionary<string, ushort[][]> { ["frame"] = [[2, 3], [4]] };
        var repartitioned = new Dictionary<string, ushort[][]> { ["frame"] = [[2], [3, 4]] };
        var swapped = new Dictionary<string, ushort[][]> { ["frame"] = [[4], [2, 3]] };
        AssertTrue(SelectedPresentationHash.FromWordFrames("test", runs) !=
            SelectedPresentationHash.FromWordFrames("test", repartitioned), "run boundaries are not flattened away");
        AssertTrue(SelectedPresentationHash.FromWordFrames("test", runs) !=
            SelectedPresentationHash.FromWordFrames("test", swapped), "native draw-run order contributes to identity");
        Console.WriteLine("  Room-actor content: all 27 domains, canonical ordering, independent edits, " +
            "dynamic item pixels/palettes, draw-run framing and host compatibility pass without a ROM.");
    }

    /// <summary>Builds stock-shaped PLM catalogs with one optional visual mutation and optional reversed insertion order.</summary>
    private static IReadOnlyDictionary<string, string> CreatePlmIdentityFixture(
        string? edited = null, bool reverse = false)
    {
        // Native definitions determine only the required shape/IDs. Copy visible words before editing;
        // the physical collision words, run geometry and native programs must remain untouched.
        IEnumerable<PlmContentIdentityFrame> Runs(string domain,
            IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions)
        {
            var frames = definitions.Select(draw => new PlmContentIdentityFrame(draw.Draw, Runs:draw.Runs.Span
                .SelectToArray(run => run.LevelWords.Span
                    .SelectToArray(word => new RoomLevelWord(word).VisualWord)))).ToArray();
            if (domain == edited)
                foreach (var frame in frames)
                    frame.Runs[^1][^1] ^= 1;
            return reverse ? frames.Reverse() : frames;
        }

        IEnumerable<PlmContentIdentityDefinition> Words(string domain,
            IEnumerable<PlmContentIdentityDefinition> definitions)
        {
            var frames = definitions.Select(draw =>
                new PlmContentIdentityDefinition(draw.Draw, Word:(ushort)(new RoomLevelWord(draw.Word).VisualWord ^
                    (domain == edited ? 1 : 0)))).ToArray();
            return reverse ? frames.Reverse() : frames;
        }

        var dynamicEntries = DynamicCollectibleGraphics.All
            .Select(graphic => new RoomPlmDynamicCollectibleArtEntry(graphic.Kind,
                graphic.Tiles.ToArray(), graphic.PaletteOffsets.ToArray())).ToArray();
        if (edited == GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName)
            dynamicEntries[^1].Tiles[^1] ^= 1;
        if (edited == "dynamic-palettes")
            dynamicEntries[^1].PaletteOffsets[^1] ^= 1;

        return RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = new InstalledRoomPlmShotBlockVisuals(Runs(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName, ShotBlockPlmDraws.All)
                .Select(draw => new RoomPlmVisualRunEntry(ShotBlockPlmDraws.VisualId(draw.Draw), draw.Runs))), GrappleBlock = new InstalledRoomPlmGrappleBlockVisuals(Words(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
                GrappleBlockPlmDraws.All.Select(draw => new PlmContentIdentityDefinition(PlmDraw.Of(draw.Frame), draw.LevelWord.Raw)))
                .Select(draw => new RoomPlmVisualWordEntry(GrappleBlockPlmDraws.VisualId(PlmDraw.FrameOf<GrappleBlockDraw>(draw.Draw, "Grapple-block")), draw.Word))), Station = new InstalledRoomPlmStationVisuals(Runs(GameInstallationLayout.RoomPlmStationVisualDirectoryName, StationPlmDraws.All)
                .Select(draw => new RoomPlmVisualRunEntry(StationPlmDraws.VisualId(draw.Draw), draw.Runs))), BlueDoor = new InstalledRoomPlmBlueDoorVisuals(Runs(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName, BlueDoorCaps.Editable)
                .Select(draw => new RoomPlmVisualEntry(BlueDoorCaps.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), ColoredDoor = new InstalledRoomPlmColoredDoorVisuals(Runs(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName, ColoredDoorCaps.All)
                .Select(draw => new RoomPlmVisualEntry(ColoredDoorCaps.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), GreyDoor = new InstalledRoomPlmGreyDoorVisuals(Runs(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName, GreyDoorCaps.All)
                .Select(draw => new RoomPlmVisualEntry(GreyDoorCaps.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), EyeDoor = new InstalledRoomPlmEyeDoorVisuals(Runs(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName, EyeDoorDraws.Editable)
                .Select(draw => new RoomPlmVisualEntry(EyeDoorDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainGlass = new RoomPlmMotherBrainGlassVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName, MotherBrainGlassPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainGlassPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), NoobTube = new InstalledRoomPlmNoobTubeVisuals(Runs(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName, NoobTubePlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(NoobTubePlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DownwardGate = new InstalledRoomPlmDownwardGateVisuals(Runs(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName, DownwardGatePlmDraws.All)
                .Select(draw => new RoomPlmVisualRunEntry(DownwardGatePlmDraws.VisualId(draw.Draw), draw.Runs))), ElevatorPlatform = new RoomPlmElevatorPlatformVisualCatalog(Runs(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName, ElevatorPlatformPlmDefinitions.DrawLists)
                .Select(draw => new RoomPlmVisualRunEntry(ElevatorPlatformPlmDefinitions.VisualId(draw.Draw), draw.Runs))), EscapeGate = new RoomPlmEscapeGateVisualCatalog(Runs(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName, MotherBrainEscapeGatePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BombTorizoHand = new RoomPlmBombTorizoHandVisualCatalog(Runs(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName, BombTorizoHandPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(BombTorizoHandPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), DraygonCannon = new RoomPlmDraygonCannonVisualCatalog(Runs(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName, DraygonCannonPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(DraygonCannonPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), ChozoStatue = new RoomPlmChozoStatueVisualCatalog(Runs(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName, ChozoStatuePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(ChozoStatuePlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), LinkedRestore = new InstalledRoomPlmLinkedRestoreVisuals(Runs(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName, RoomPlmLinkedRestoreDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(LinkedRestorePlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), TourianAccess = new RoomPlmTourianAccessVisualCatalog(Runs(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName, TourianAccessPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(TourianAccessPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SpeedBooster = new InstalledRoomPlmSpeedBoosterVisuals(Runs(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName, SpeedBoosterBlockPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(SpeedBoosterBlockPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MaridiaElevatube = new InstalledRoomPlmMaridiaElevatubeVisuals(Runs(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName, MaridiaElevatubePlmDefinitions.AllDraws)
                .Select(draw => new RoomPlmVisualEntry(MaridiaElevatubeDraws.DrawVisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SporeSpawnCeiling = new RoomPlmSporeSpawnCeilingVisualCatalog(Runs(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName, SporeSpawnCeilingPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(SporeSpawnCeilingPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), SamusEater = new InstalledRoomPlmSamusEaterVisuals(Runs(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName, SamusEaterPlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(SamusEaterPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), BotwoonWall = new InstalledRoomPlmBotwoonWallVisuals(Runs(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName, BotwoonWallPlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(BotwoonWallPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Kraid = new InstalledRoomPlmKraidVisuals(Runs(GameInstallationLayout.RoomPlmKraidVisualDirectoryName, KraidRoomPlmDraws.All)
                .Select(draw => new RoomPlmVisualEntry(KraidRoomPlmDraws.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Crocomire = new RoomPlmCrocomireVisualCatalog(Runs(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName, CrocomireArenaPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(CrocomireArenaPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainFakeDeath = new RoomPlmMotherBrainFakeDeathVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName, MotherBrainFakeDeathPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainFakeDeathPlmDrawDefinitions.VisualId(draw.Draw), draw.Runs.SelectMany(run => run).ToArray()))), Collectible = new InstalledRoomPlmCollectibleVisuals(Words(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
                CollectiblePlmDraws.All.Select(draw => new PlmContentIdentityDefinition(PlmDraw.Of(draw.Draw), draw.LevelWord.Raw)))
                .Select(draw => new RoomPlmVisualWordEntry(CollectiblePlmDraws.VisualId(PlmDraw.FrameOf<RoomPlmCollectibleDrawDefinitions.Draw>(draw.Draw, "collectible")), draw.Word))), DynamicCollectibleArt = new InstalledDynamicCollectibleArt(reverse ? dynamicEntries.Reverse() : dynamicEntries) });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int PlmContentIdentityCommand()
    {
        Suite(nameof(VerifyPlmContentIdentity), () => VerifyPlmContentIdentity());
        return 0;
    }
}
