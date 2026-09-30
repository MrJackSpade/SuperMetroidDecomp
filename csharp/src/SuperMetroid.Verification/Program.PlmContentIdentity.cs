using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Selected-art verification only: constructed catalogs, no ROM and no gameplay traversal.</summary>
    private static void VerifyPlmContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreatePlmIdentityFixture();
        IReadOnlyDictionary<string, string> stock = RoomPlmPresentationIdentity.Create(
            RoomPlmShotBlockVisualCatalog.Stock(),
            RoomPlmGrappleBlockVisualCatalog.Stock(),
            RoomPlmStationVisualCatalog.Stock(),
            RoomPlmBlueDoorVisualCatalog.Stock(),
            RoomPlmColoredDoorVisualCatalog.Stock(),
            RoomPlmGreyDoorVisualCatalog.Stock(),
            RoomPlmEyeDoorVisualCatalog.Stock(),
            RoomPlmMotherBrainGlassVisualCatalog.Stock(),
            RoomPlmNoobTubeVisualCatalog.Stock(),
            RoomPlmDownwardGateVisualCatalog.Stock(),
            RoomPlmElevatorPlatformVisualCatalog.Stock(),
            RoomPlmEscapeGateVisualCatalog.Stock(),
            RoomPlmBombTorizoHandVisualCatalog.Stock(),
            RoomPlmDraygonCannonVisualCatalog.Stock(),
            RoomPlmChozoStatueVisualCatalog.Stock(),
            RoomPlmLinkedRestoreVisualCatalog.Stock(),
            RoomPlmTourianAccessVisualCatalog.Stock(),
            RoomPlmSpeedBoosterVisualCatalog.Stock(),
            RoomPlmMaridiaElevatubeVisualCatalog.Stock(),
            RoomPlmSporeSpawnCeilingVisualCatalog.Stock(),
            RoomPlmSamusEaterVisualCatalog.Stock(),
            RoomPlmBotwoonWallVisualCatalog.Stock(),
            RoomPlmKraidVisualCatalog.Stock(),
            RoomPlmCrocomireVisualCatalog.Stock(),
            RoomPlmMotherBrainFakeDeathVisualCatalog.Stock(),
            RoomPlmCollectibleVisualCatalog.Stock(),
            RoomPlmDynamicCollectibleArtCatalog.Stock());
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

    private static IReadOnlyDictionary<string, string> CreatePlmIdentityFixture(
        string? edited = null, bool reverse = false)
    {
        // Native definitions determine only the required shape/IDs. Copy visible words before editing;
        // the physical collision words, run geometry and native programs must remain untouched.
        IEnumerable<(ushort Pointer, ushort[][] Runs)> Runs(string domain,
            IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions)
        {
            var frames = definitions.Select(draw => (draw.Pointer, Runs: draw.Runs.Span.ToArray()
                .Select(run => run.LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray()).ToArray())).ToArray();
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

        var dynamicEntries = RoomPlmDynamicCollectibleGraphicsDefinitions.All.ToArray()
            .Select(graphic => new RoomPlmDynamicCollectibleArtEntry(graphic.Kind,
                graphic.Tiles.ToArray(), graphic.PaletteOffsets.ToArray())).ToArray();
        if (edited == GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName)
            dynamicEntries[^1].Tiles[^1] ^= 1;
        if (edited == "dynamic-palettes")
            dynamicEntries[^1].PaletteOffsets[^1] ^= 1;

        return RoomPlmPresentationIdentity.Create(
            new RoomPlmShotBlockVisualCatalog(Runs(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName, RoomPlmShotBlockDrawDefinitions.All)
                .Select(draw => new RoomPlmShotBlockVisualEntry(draw.Pointer, draw.Runs))),
            new RoomPlmGrappleBlockVisualCatalog(Words(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
                RoomPlmGrappleBlockDrawDefinitions.All.Select(draw => (draw.Pointer, draw.LevelWord)))
                .Select(draw => new RoomPlmGrappleBlockVisualEntry(draw.Pointer, draw.Word))),
            new RoomPlmStationVisualCatalog(Runs(GameInstallationLayout.RoomPlmStationVisualDirectoryName, RoomPlmStationDrawDefinitions.All)
                .Select(draw => new RoomPlmStationVisualEntry(RoomPlmStationDrawDefinitions.VisualId(draw.Pointer), draw.Runs))),
            new RoomPlmBlueDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName, BlueDoorPlmDrawDefinitions.Editable)
                .Select(draw => new RoomPlmBlueDoorVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmColoredDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName, ColoredDoorPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmColoredDoorVisualEntry(ColoredDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmGreyDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName, GreyDoorPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmGreyDoorVisualEntry(GreyDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmEyeDoorVisualCatalog(Runs(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName, EyeDoorPlmDrawDefinitions.Editable)
                .Select(draw => new RoomPlmEyeDoorVisualEntry(EyeDoorPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmMotherBrainGlassVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName, MotherBrainGlassPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmMotherBrainGlassVisualEntry(MotherBrainGlassPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmNoobTubeVisualCatalog(Runs(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName, NoobTubePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmNoobTubeVisualEntry(NoobTubePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmDownwardGateVisualCatalog(Runs(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName, DownwardGatePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmDownwardGateVisualEntry(DownwardGatePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs))),
            new RoomPlmElevatorPlatformVisualCatalog(Runs(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName, ElevatorPlatformPlmDefinitions.DrawLists)
                .Select(draw => new RoomPlmElevatorPlatformVisualEntry(ElevatorPlatformPlmDefinitions.VisualId(draw.Pointer), draw.Runs))),
            new RoomPlmEscapeGateVisualCatalog(Runs(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName, MotherBrainEscapeGatePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmEscapeGateVisualEntry(MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmBombTorizoHandVisualCatalog(Runs(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName, BombTorizoHandPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmBombTorizoHandVisualEntry(BombTorizoHandPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmDraygonCannonVisualCatalog(Runs(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName, DraygonCannonPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmDraygonCannonVisualEntry(DraygonCannonPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmChozoStatueVisualCatalog(Runs(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName, ChozoStatuePlmDrawDefinitions.All)
                .Select(draw => new RoomPlmChozoStatueVisualEntry(ChozoStatuePlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmLinkedRestoreVisualCatalog(Runs(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName, RoomPlmLinkedRestoreDrawDefinitions.All)
                .Select(draw => new RoomPlmLinkedRestoreVisualEntry(RoomPlmLinkedRestoreDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmTourianAccessVisualCatalog(Runs(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName, TourianAccessPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmTourianAccessVisualEntry(TourianAccessPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmSpeedBoosterVisualCatalog(Runs(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName, SpeedBoosterBlockPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmSpeedBoosterVisualEntry(SpeedBoosterBlockPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmMaridiaElevatubeVisualCatalog(Runs(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName, MaridiaElevatubePlmDefinitions.AllDraws)
                .Select(draw => new RoomPlmMaridiaElevatubeVisualEntry(MaridiaElevatubePlmDefinitions.DrawVisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmSporeSpawnCeilingVisualCatalog(Runs(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName, SporeSpawnCeilingPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmSporeSpawnCeilingVisualEntry(SporeSpawnCeilingPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmSamusEaterVisualCatalog(Runs(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName, SamusEaterPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmSamusEaterVisualEntry(SamusEaterPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmBotwoonWallVisualCatalog(Runs(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName, BotwoonWallPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmBotwoonWallVisualEntry(BotwoonWallPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmKraidVisualCatalog(Runs(GameInstallationLayout.RoomPlmKraidVisualDirectoryName, KraidRoomPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmKraidVisualEntry(KraidRoomPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmCrocomireVisualCatalog(Runs(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName, CrocomireArenaPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmCrocomireVisualEntry(CrocomireArenaPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmMotherBrainFakeDeathVisualCatalog(Runs(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName, MotherBrainFakeDeathPlmDrawDefinitions.All)
                .Select(draw => new RoomPlmMotherBrainFakeDeathVisualEntry(MotherBrainFakeDeathPlmDrawDefinitions.VisualId(draw.Pointer), draw.Runs.SelectMany(run => run).ToArray()))),
            new RoomPlmCollectibleVisualCatalog(Words(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
                RoomPlmCollectibleDrawDefinitions.All.ToArray().Select(draw => (draw.Pointer, draw.LevelWord)))
                .Select(draw => new RoomPlmCollectibleVisualEntry(RoomPlmCollectibleDrawDefinitions.All.ToArray().Single(frame => frame.Pointer == draw.Pointer).Id, draw.Word))),
            new RoomPlmDynamicCollectibleArtCatalog(reverse ? dynamicEntries.Reverse() : dynamicEntries));
    }
}
