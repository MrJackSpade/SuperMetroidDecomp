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
        IReadOnlyDictionary<string, string> stock = RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = RoomPlmShotBlockVisualCatalog.Stock(), GrappleBlock = RoomPlmGrappleBlockVisualCatalog.Stock(), Station = RoomPlmStationVisualCatalog.Stock(), BlueDoor = RoomPlmBlueDoorVisualCatalog.Stock(), ColoredDoor = RoomPlmColoredDoorVisualCatalog.Stock(), GreyDoor = RoomPlmGreyDoorVisualCatalog.Stock(), EyeDoor = RoomPlmEyeDoorVisualCatalog.Stock(), MotherBrainGlass = RoomPlmMotherBrainGlassVisualCatalog.Stock(), NoobTube = RoomPlmNoobTubeVisualCatalog.Stock(), DownwardGate = RoomPlmDownwardGateVisualCatalog.Stock(), ElevatorPlatform = RoomPlmElevatorPlatformVisualCatalog.Stock(), EscapeGate = RoomPlmEscapeGateVisualCatalog.Stock(), BombTorizoHand = RoomPlmBombTorizoHandVisualCatalog.Stock(), DraygonCannon = RoomPlmDraygonCannonVisualCatalog.Stock(), ChozoStatue = RoomPlmChozoStatueVisualCatalog.Stock(), LinkedRestore = RoomPlmLinkedRestoreVisualCatalog.Stock(), TourianAccess = RoomPlmTourianAccessVisualCatalog.Stock(), SpeedBooster = RoomPlmSpeedBoosterVisualCatalog.Stock(), MaridiaElevatube = RoomPlmMaridiaElevatubeVisualCatalog.Stock(), SporeSpawnCeiling = RoomPlmSporeSpawnCeilingVisualCatalog.Stock(), SamusEater = RoomPlmSamusEaterVisualCatalog.Stock(), BotwoonWall = RoomPlmBotwoonWallVisualCatalog.Stock(), Kraid = RoomPlmKraidVisualCatalog.Stock(), Crocomire = RoomPlmCrocomireVisualCatalog.Stock(), MotherBrainFakeDeath = RoomPlmMotherBrainFakeDeathVisualCatalog.Stock(), Collectible = RoomPlmCollectibleVisualCatalog.Stock(), DynamicCollectibleArt = RoomPlmDynamicCollectibleArtCatalog.Stock() });
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

        var runs = new Dictionary<ushort, ushort[][]> { [1] = [[2, 3], [4]] };
        var repartitioned = new Dictionary<ushort, ushort[][]> { [1] = [[2], [3, 4]] };
        var swapped = new Dictionary<ushort, ushort[][]> { [1] = [[4], [2, 3]] };
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
        IEnumerable<(ushort Pointer, ushort[][] Runs)> Runs(string domain,
            IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions)
        {
            var frames = definitions.Select(draw => (draw.Pointer, Runs: draw.Runs.Span
                .SelectToArray(run => run.LevelWords.Span
                    .SelectToArray(word => new RoomLevelWord(word).VisualWord)))).ToArray();
            if (domain == edited)
                foreach (var frame in frames)
                    frame.Runs[^1][^1] ^= 1;
            return reverse ? frames.Reverse() : frames;
        }

        IEnumerable<(ushort Pointer, ushort Word)> Words(string domain,
            IEnumerable<(ushort Pointer, ushort Word)> definitions)
        {
            var frames = definitions.Select(draw =>
                (draw.Pointer, Word: (ushort)(new RoomLevelWord(draw.Word).VisualWord ^
                    (domain == edited ? 1 : 0)))).ToArray();
            return reverse ? frames.Reverse() : frames;
        }

        var dynamicEntries = RoomPlmDynamicCollectibleGraphicsDefinitions.All
            .Select(graphic => new RoomPlmDynamicCollectibleArtEntry(graphic.Kind,
                graphic.Tiles.ToArray(), graphic.PaletteOffsets.ToArray())).ToArray();
        if (edited == GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName)
            dynamicEntries[^1].Tiles[^1] ^= 1;
        if (edited == "dynamic-palettes")
            dynamicEntries[^1].PaletteOffsets[^1] ^= 1;

        return RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet { ShotBlock = new RoomPlmShotBlockVisualCatalog(Runs(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName, RoomPlmShotBlockDrawDefinitions.All)
                .Select(draw => new RoomPlmShotBlockVisualEntry(draw.Pointer, draw.Runs))), GrappleBlock = new RoomPlmGrappleBlockVisualCatalog(Words(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
                RoomPlmGrappleBlockDrawDefinitions.All.Select(draw => (draw.Pointer, draw.LevelWord)))
                .Select(draw => new RoomPlmGrappleBlockVisualEntry(draw.Pointer, draw.Word))), Station = new RoomPlmStationVisualCatalog(Runs(GameInstallationLayout.RoomPlmStationVisualDirectoryName, RoomPlmStationDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualRunEntry(RoomPlmStationDrawDefinitions.VisualId(draw.Pointer), draw.Runs))), BlueDoor = new RoomPlmBlueDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName, BlueDoorPlmDrawDefinitions.Editable)
                .Select(draw => new RoomPlmVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), ColoredDoor = new RoomPlmColoredDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName, ColoredDoorPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(ColoredDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), GreyDoor = new RoomPlmGreyDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName, GreyDoorPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(GreyDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), EyeDoor = new RoomPlmEyeDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName, EyeDoorPlmDrawDefinitions.Editable)
                .Select(draw => new RoomPlmVisualEntry(EyeDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainGlass = new RoomPlmMotherBrainGlassVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName, MotherBrainGlassPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainGlassPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), NoobTube = new RoomPlmNoobTubeVisualCatalog(Runs(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName, NoobTubePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(NoobTubePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), DownwardGate = new RoomPlmDownwardGateVisualCatalog(Runs(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName, DownwardGatePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualRunEntry(DownwardGatePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs))), ElevatorPlatform = new RoomPlmElevatorPlatformVisualCatalog(Runs(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName, ElevatorPlatformPlmDefinitions.DrawLists)
                .Select(draw => new RoomPlmVisualRunEntry(ElevatorPlatformPlmDefinitions.VisualId(draw.Pointer), draw.Runs))), EscapeGate = new RoomPlmEscapeGateVisualCatalog(Runs(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName, MotherBrainEscapeGatePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), BombTorizoHand = new RoomPlmBombTorizoHandVisualCatalog(Runs(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName, BombTorizoHandPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(BombTorizoHandPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), DraygonCannon = new RoomPlmDraygonCannonVisualCatalog(Runs(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName, DraygonCannonPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(DraygonCannonPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), ChozoStatue = new RoomPlmChozoStatueVisualCatalog(Runs(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName, ChozoStatuePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(ChozoStatuePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), LinkedRestore = new RoomPlmLinkedRestoreVisualCatalog(Runs(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName, RoomPlmLinkedRestoreDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(RoomPlmLinkedRestoreDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), TourianAccess = new RoomPlmTourianAccessVisualCatalog(Runs(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName, TourianAccessPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(TourianAccessPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), SpeedBooster = new RoomPlmSpeedBoosterVisualCatalog(Runs(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName, SpeedBoosterBlockPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(SpeedBoosterBlockPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), MaridiaElevatube = new RoomPlmMaridiaElevatubeVisualCatalog(Runs(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName, MaridiaElevatubePlmDefinitions.AllDraws)
                .Select(draw => new RoomPlmVisualEntry(MaridiaElevatubePlmDefinitions.DrawVisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), SporeSpawnCeiling = new RoomPlmSporeSpawnCeilingVisualCatalog(Runs(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName, SporeSpawnCeilingPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(SporeSpawnCeilingPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), SamusEater = new RoomPlmSamusEaterVisualCatalog(Runs(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName, SamusEaterPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(SamusEaterPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), BotwoonWall = new RoomPlmBotwoonWallVisualCatalog(Runs(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName, BotwoonWallPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(BotwoonWallPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), Kraid = new RoomPlmKraidVisualCatalog(Runs(GameInstallationLayout.RoomPlmKraidVisualDirectoryName, KraidRoomPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(KraidRoomPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), Crocomire = new RoomPlmCrocomireVisualCatalog(Runs(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName, CrocomireArenaPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(CrocomireArenaPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), MotherBrainFakeDeath = new RoomPlmMotherBrainFakeDeathVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName, MotherBrainFakeDeathPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmVisualEntry(MotherBrainFakeDeathPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))), Collectible = new RoomPlmCollectibleVisualCatalog(Words(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
                RoomPlmCollectibleDrawDefinitions.All.Select(draw => (checked((ushort)draw.Pointer), draw.LevelWord.Raw)))
                .Select(draw => new RoomPlmCollectibleVisualEntry(RoomPlmCollectibleDrawDefinitions.All.Single(frame => frame.Pointer == draw.Pointer).Id, draw.Word))), DynamicCollectibleArt = new RoomPlmDynamicCollectibleArtCatalog(reverse ? dynamicEntries.Reverse() : dynamicEntries) });
    }

    /// <summary>Runs the ROM-free selected-art identity and compatibility checks.</summary>
    [VerificationCommand("--plm-content-identity")]
    internal static int PlmContentIdentityCommand()
    {
        Suite(nameof(VerifyPlmContentIdentity), () => VerifyPlmContentIdentity());
        return 0;
    }
}
